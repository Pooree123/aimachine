using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;
using Aimachine.Services; // ✅ เพิ่ม

namespace Aimachine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionsController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IWebHostEnvironment _environment; // (ยังเก็บไว้ เผื่อส่วนอื่นใช้)
        private readonly CloudinaryService _cloud;         // ✅ เพิ่ม

        public SolutionsController(
            AimachineContext context,
            IWebHostEnvironment environment,
            CloudinaryService cloud) // ✅ เพิ่ม
        {
            _context = context;
            _environment = environment;
            _cloud = cloud;
        }

        // ✅ Helper Function: เช็คไฟล์รูป + ขนาดไฟล์
        private bool IsAllowedImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            // 1. เช็คขนาดไฟล์ (5 MB)
            if (file.Length > 5 * 1024 * 1024) return false;

            // 2. เช็คนามสกุลและ MIME type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/jpg" };

            var ext = Path.GetExtension(file.FileName).ToLower();
            var mime = file.ContentType.ToLower();

            return allowedExtensions.Contains(ext) && allowedMimeTypes.Contains(mime);
        }

        // ✅ Upload ขึ้น Cloudinary แล้วคืน URL
        private Task<string> SaveImageAsync(IFormFile file)
            => _cloud.UploadImageAsync(file, "aimachine/solutions");

        // ✅ PUBLIC
        [HttpGet]
        public async Task<IActionResult> GetPublicSolutions()
        {
            var data = await _context.DepartmentTypes
                .AsNoTracking()
                .Where(d => d.Solutions.Any(s => s.Status == "Active"))
                .Select(d => new
                {
                    DepartmentId = d.Id,
                    DepartmentTitle = d.DepartmentTitle,

                    TechStackGroups = d.TechStackTags
                        .Select(tag => new
                        {
                            TagId = tag.Id,
                            TagTitle = tag.TechStackTitle
                        })
                        .ToList(),

                    Solutions = d.Solutions
                        .Where(s => s.Status == "Active")
                        .OrderByDescending(s => s.Id)
                        .Select(s => new
                        {
                            s.Id,
                            s.DepartmentId,
                            s.Name,
                            s.Status,
                            DepartmentTitle = d.DepartmentTitle,
                            Images = s.SolutionImgs
                                .Where(img => img.IsCover == true)
                                .Select(img => new
                                {
                                    img.Id,
                                    Url = string.IsNullOrEmpty(img.Image) ? null : img.Image, // ✅ URL ตรง
                                    img.IsCover
                                })
                                .ToList(),
                            s.CreatedAt,
                            s.UpdateAt
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ ADMIN
        [HttpGet("admin")]
        [Authorize]
        public async Task<IActionResult> GetAdminSolutions()
        {
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
                    s.Status,
                    DepartmentTitle = s.Department != null ? s.Department.DepartmentTitle : "",
                    Images = s.SolutionImgs
                        .OrderBy(img => img.OrderId)
                        .Select(img => new
                        {
                            img.Id,
                            Url = string.IsNullOrEmpty(img.Image) ? null : img.Image, // ✅ URL ตรง
                            img.IsCover,
                            img.OrderId
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
            var s = await _context.Solutions
                .AsNoTracking()
                .Include(d => d.Department)
                .Include(i => i.SolutionImgs)
                .Where(x => x.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Status,
                    s.DepartmentId,
                    DepartmentName = s.Department != null ? s.Department.DepartmentTitle : "",
                    Images = s.SolutionImgs
                        .Select(img => new
                        {
                            img.Id,
                            Url = string.IsNullOrEmpty(img.Image) ? null : img.Image, // ✅ URL ตรง
                            img.IsCover
                        })
                        .OrderByDescending(i => i.IsCover)
                        .ToList(),
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

            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                foreach (var img in dto.ImageFiles)
                {
                    if (!IsAllowedImageFile(img))
                        return BadRequest(new { Message = $"ไฟล์ '{img.FileName}' ไม่ถูกต้อง (ต้องเป็น .jpg/.png และขนาดไม่เกิน 5MB)" });
                }
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var solution = new Solution
                    {
                        DepartmentId = dto.DepartmentId,
                        Name = dto.Name,
                        Status = dto.Status,
                        CreatedBy = currentUserId,
                        UpdateBy = currentUserId,
                        CreatedAt = DateTime.UtcNow.AddHours(7),
                        UpdateAt = DateTime.UtcNow.AddHours(7)
                    };

                    _context.Solutions.Add(solution);
                    await _context.SaveChangesAsync();

                    // ✅ Upload รูปขึ้น Cloudinary แล้วเก็บ URL ใน DB
                    if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
                    {
                        bool isFirstImage = true;

                        foreach (var file in dto.ImageFiles.Where(f => f != null && f.Length > 0))
                        {
                            var imageUrl = await SaveImageAsync(file);

                            var imgEntity = new SolutionImg
                            {
                                SolutionId = solution.Id,
                                Image = imageUrl, // ✅ เก็บ URL
                                IsCover = isFirstImage,
                                OrderId = 0
                            };

                            _context.SolutionImgs.Add(imgEntity);
                            isFirstImage = false;
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

            if (dto.NewImageFiles != null && dto.NewImageFiles.Count > 0)
            {
                foreach (var img in dto.NewImageFiles)
                {
                    if (!IsAllowedImageFile(img))
                        return BadRequest(new { Message = $"ไฟล์ '{img.FileName}' ไม่ถูกต้อง (ต้องเป็น .jpg/.png และขนาดไม่เกิน 5MB)" });
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

                    entity.DepartmentId = dto.DepartmentId;
                    entity.Name = dto.Name;
                    entity.Status = dto.Status;
                    entity.UpdateBy = currentUserId;
                    entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                    // ✅ Upload รูปใหม่ขึ้น Cloudinary + เก็บ URL
                    if (dto.NewImageFiles != null && dto.NewImageFiles.Count > 0)
                    {
                        // (ตามโค้ดเดิมคุณลบ cover เก่า 1 รูปก่อน)
                        var oldCover = entity.SolutionImgs.FirstOrDefault(img => img.IsCover == true);
                        if (oldCover != null)
                        {
                            // ✅ ตอนนี้ลบแค่ DB record (ถ้าจะลบบน Cloudinary ต้องเก็บ public_id เพิ่ม)
                            _context.SolutionImgs.Remove(oldCover);
                        }

                        bool isFirstNewImage = true;
                        foreach (var file in dto.NewImageFiles.Where(f => f != null && f.Length > 0))
                        {
                            var imageUrl = await SaveImageAsync(file);

                            var imgEntity = new SolutionImg
                            {
                                SolutionId = entity.Id,
                                Image = imageUrl, // ✅ เก็บ URL
                                IsCover = isFirstNewImage,
                                OrderId = 0
                            };

                            _context.SolutionImgs.Add(imgEntity);
                            isFirstNewImage = false;
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

                if (entity.SolutionImgs != null && entity.SolutionImgs.Count > 0)
                {
                    // ✅ ลบ record ใน DB (ไม่ลบ Cloudinary เพราะยังไม่มี public_id)
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

        [HttpDelete("image/{imgId:int}")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            var img = await _context.SolutionImgs.FindAsync(imgId);
            if (img == null) return NotFound(new { Message = "ไม่พบรูปภาพ" });

            // ✅ ลบแค่ DB ก่อน
            _context.SolutionImgs.Remove(img);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบรูปภาพสำเร็จ" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SolutionSearchQueryDto req)
        {
            try
            {
                var query = _context.Solutions
                    .AsNoTracking()
                    .Include(s => s.Department)
                    .Include(s => s.SolutionImgs)
                    .AsQueryable();

                if (req.DepartmentId.HasValue)
                    query = query.Where(s => s.DepartmentId == req.DepartmentId.Value);

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
                        EF.Functions.Collate((s.Name ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
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
                        s.Status,
                        s.CreatedAt,
                        CoverUrl = s.SolutionImgs
                            .OrderByDescending(img => img.IsCover)
                            .ThenBy(img => img.Id)
                            .Select(img => string.IsNullOrEmpty(img.Image) ? null : img.Image) // ✅ URL ตรง
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
