using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionsController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IWebHostEnvironment _environment;

        public SolutionsController(AimachineContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ✅ เพิ่ม Helper Function เช็คไฟล์รูป
        private bool IsAllowedImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/jpg" };

            var ext = Path.GetExtension(file.FileName).ToLower();
            var mime = file.ContentType.ToLower();

            return allowedExtensions.Contains(ext) && allowedMimeTypes.Contains(mime);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var data = await _context.Solutions
                .AsNoTracking()
                .Include(s => s.Department)
                .Include(s => s.SolutionImgs)
                .OrderByDescending(s => s.Id)
                .Select(s => new
                {
                    s.Id,
                    s.DepartmentId,
                    s.Name,
                    s.Description,
                    s.Status,
                    DepartmentTitle = s.Department != null ? s.Department.DepartmentTitle : "",

                    Images = s.SolutionImgs
                        .Where(img => img.IsCover == true)
                        .Select(img => new
                        {
                            img.Id,
                            Url = string.IsNullOrEmpty(img.Image) ? null : $"{baseUrl}/{img.Image}",
                            img.IsCover
                        })
                        .ToList(),

                    s.CreatedAt,
                    s.UpdateAt
                })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var s = await _context.Solutions
                .AsNoTracking()
                .Include(d => d.Department)
                .Include(i => i.SolutionImgs)
                .Where(x => x.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Status,
                    s.DepartmentId,
                    DepartmentName = s.Department != null ? s.Department.DepartmentTitle : "",
                    Images = s.SolutionImgs.Select(img => new
                    {
                        img.Id,
                        Url = string.IsNullOrEmpty(img.Image) ? null : $"{baseUrl}/{img.Image}",
                        img.IsCover
                    }).OrderByDescending(i => i.IsCover).ToList(),
                    s.CreatedAt,
                    s.UpdateAt
                })
                .FirstOrDefaultAsync();

            if (s == null) return NotFound(new { Message = "ไม่พบข้อมูล Solution" });
            return Ok(s);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateSolutionDto dto)
        {
            int currentUserId = User.GetUserId();

            if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
                return BadRequest(new { Message = "ไม่พบ Department ID นี้ในระบบ" });

            // 🛡️ ตรวจสอบไฟล์รูปภาพ (Validation)
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                foreach (var img in dto.ImageFiles)
                {
                    if (!IsAllowedImageFile(img))
                    {
                        return BadRequest(new { Message = $"ไฟล์ '{img.FileName}' ไม่ถูกต้อง อนุญาตเฉพาะ .jpg, .jpeg, .png เท่านั้น" });
                    }
                }
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. บันทึก Solution
                    var solution = new Solution
                    {
                        DepartmentId = dto.DepartmentId,
                        Name = dto.Name,
                        Description = dto.Description,
                        Status = dto.Status,
                        CreatedBy = currentUserId,
                        UpdateBy = currentUserId,
                        CreatedAt = DateTime.UtcNow.AddHours(7),
                        UpdateAt = DateTime.UtcNow.AddHours(7)
                    };

                    _context.Solutions.Add(solution);
                    await _context.SaveChangesAsync();

                    // 2. บันทึกรูปภาพ
                    if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "solutions");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        bool isFirstImage = true;
                        foreach (var file in dto.ImageFiles)
                        {
                            if (file.Length > 0)
                            {
                                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                string filePath = Path.Combine(uploadsFolder, fileName);
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var imgEntity = new SolutionImg
                                {
                                    SolutionId = solution.Id,
                                    Image = $"uploads/solutions/{fileName}",
                                    IsCover = isFirstImage,
                                    OrderId = 0
                                };
                                _context.SolutionImgs.Add(imgEntity);
                                isFirstImage = false;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return Ok(new { Message = "เพิ่ม Solution สำเร็จ", Id = solution.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
                }
            });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateSolutionDto dto)
        {
            int currentUserId = User.GetUserId();
            var strategy = _context.Database.CreateExecutionStrategy();

            // 🛡️ ตรวจสอบไฟล์รูปภาพใหม่ (Validation)
            if (dto.NewImageFiles != null && dto.NewImageFiles.Count > 0)
            {
                foreach (var img in dto.NewImageFiles)
                {
                    if (!IsAllowedImageFile(img))
                    {
                        return BadRequest(new { Message = $"ไฟล์ '{img.FileName}' ไม่ถูกต้อง อนุญาตเฉพาะ .jpg, .jpeg, .png เท่านั้น" });
                    }
                }
            }

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var entity = await _context.Solutions
                        .Include(s => s.SolutionImgs)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Solution" });

                    if (entity.DepartmentId != dto.DepartmentId)
                    {
                        if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
                            return BadRequest(new { Message = "ไม่พบ Department ID ที่ระบุ" });
                    }

                    // อัปเดต Text
                    entity.DepartmentId = dto.DepartmentId;
                    entity.Name = dto.Name;
                    entity.Description = dto.Description;
                    entity.Status = dto.Status;
                    entity.UpdateBy = currentUserId;
                    entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                    // ✅ อัปเดต รูปภาพ (Logic ใหม่: แทนที่รูปปก)
                    if (dto.NewImageFiles != null && dto.NewImageFiles.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "solutions");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        // 1. หาและลบรูปปกเก่า
                        var oldCover = entity.SolutionImgs.FirstOrDefault(img => img.IsCover == true);
                        if (oldCover != null)
                        {
                            // ลบไฟล์จริง
                            if (!string.IsNullOrEmpty(oldCover.Image))
                            {
                                string oldPath = Path.Combine(_environment.WebRootPath, oldCover.Image);
                                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                            }
                            // ลบจาก Database
                            _context.SolutionImgs.Remove(oldCover);
                        }

                        // 2. เพิ่มรูปใหม่ และตั้งรูปแรกเป็น Cover
                        bool isFirstNewImage = true;

                        foreach (var file in dto.NewImageFiles)
                        {
                            if (file.Length > 0)
                            {
                                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                string filePath = Path.Combine(uploadsFolder, fileName);
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var imgEntity = new SolutionImg
                                {
                                    SolutionId = entity.Id,
                                    Image = $"uploads/solutions/{fileName}",
                                    IsCover = isFirstNewImage,
                                    OrderId = 0
                                };

                                _context.SolutionImgs.Add(imgEntity);
                                isFirstNewImage = false;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "แก้ไขไม่สำเร็จ", Error = ex.Message });
                }
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Solutions
                    .Include(s => s.SolutionImgs)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล" });

                if (entity.SolutionImgs != null)
                {
                    foreach (var img in entity.SolutionImgs)
                    {
                        if (!string.IsNullOrEmpty(img.Image))
                        {
                            string fullPath = Path.Combine(_environment.WebRootPath, img.Image);
                            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                        }
                    }
                    _context.SolutionImgs.RemoveRange(entity.SolutionImgs);
                }

                _context.Solutions.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบไม่สำเร็จ", Error = ex.Message });
            }
        }

        // DELETE Image
        [HttpDelete("image/{imgId:int}")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.SolutionImgs.FindAsync(imgId);
            if (img == null) return NotFound(new { Message = "ไม่พบรูปภาพ" });

            if (!string.IsNullOrEmpty(img.Image))
            {
                string fullPath = Path.Combine(_environment.WebRootPath, img.Image);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            _context.SolutionImgs.Remove(img);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบรูปภาพสำเร็จ" });
        }

        // ... (Search เหมือนเดิม) ...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SolutionSearchQueryDto req)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                var query = _context.Solutions
                    .AsNoTracking()
                    .Include(s => s.Department)
                    .Include(s => s.SolutionImgs)
                    .AsQueryable();

                if (req.DepartmentId.HasValue)
                {
                    query = query.Where(s => s.DepartmentId == req.DepartmentId.Value);
                }

                if (!string.IsNullOrWhiteSpace(req.DepartmentTitle))
                {
                    var deptTitle = req.DepartmentTitle.Trim();
                    query = query.Where(s => s.Department != null &&
                        EF.Functions.Collate(s.Department.DepartmentTitle, "SQL_Latin1_General_CP1_CI_AS") == deptTitle);
                }

                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();
                    query = query.Where(s =>
                        EF.Functions.Collate((s.Name ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        EF.Functions.Collate((s.Description ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                    );
                }

                var data = await query
                    .OrderByDescending(s => s.Id)
                    .Select(s => new
                    {
                        s.Id,
                        s.DepartmentId,
                        DepartmentTitle = s.Department != null ? s.Department.DepartmentTitle : "",
                        s.Name,
                        s.Description,
                        s.Status,
                        s.CreatedAt,
                        CoverUrl = s.SolutionImgs
                            .OrderByDescending(img => img.IsCover)
                            .ThenBy(img => img.Id)
                            .Select(img => string.IsNullOrEmpty(img.Image) ? null : $"{baseUrl}/{img.Image}")
                            .FirstOrDefault(),
                        ImagesCount = s.SolutionImgs.Count()
                    })
                    .ToListAsync();

                return Ok(new { Message = "ค้นหาสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ค้นหาไม่สำเร็จ", Error = ex.Message });
            }
        }
    }
}