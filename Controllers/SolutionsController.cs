using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // =============================================
        // GET: GetAll
        // =============================================
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
                    s.Name,
                    s.Description,
                    s.Status,
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
                .ToListAsync();
            return Ok(data);
        }

        // =============================================
        // GET: GetById
        // =============================================
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

        // =============================================
        // ✅ POST: Create (แก้ไขแล้ว)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSolutionDto dto)
        {
            if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
                return BadRequest(new { Message = "ไม่พบ Department ID นี้ในระบบ" });

            var strategy = _context.Database.CreateExecutionStrategy();

            // ✅ ระบุ <IActionResult> ชัดเจน เพื่อแก้ Error CS8031
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
                        CreatedBy = dto.CreatedBy,
                        UpdateBy = dto.CreatedBy,
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

        // =============================================
        // ✅ PUT: Update (แก้ไขแล้ว)
        // =============================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateSolutionDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            // ✅ ระบุ <IActionResult> ชัดเจน เพื่อแก้ Error CS8031
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
                    entity.UpdateBy = dto.UpdateBy;
                    entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                    // อัปเดต รูปภาพ
                    if (dto.NewImageFiles != null && dto.NewImageFiles.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "solutions");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        bool hasCover = entity.SolutionImgs.Any(img => img.IsCover == true);

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

                                bool isThisImageCover = !hasCover;
                                var imgEntity = new SolutionImg
                                {
                                    SolutionId = entity.Id,
                                    Image = $"uploads/solutions/{fileName}",
                                    IsCover = isThisImageCover,
                                    OrderId = 0
                                };
                                _context.SolutionImgs.Add(imgEntity);
                                if (isThisImageCover) hasCover = true;
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

        // =============================================
        // DELETE: Delete (ไม่ต้องใช้ Transaction เพราะลบตรงๆ)
        // =============================================
        [HttpDelete("{id:int}")]
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


        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? status)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                // ✅ Include รูป เพื่อเอา cover url และ count รูป
                var query = _context.Solutions
                    .AsNoTracking()
                    .Include(s => s.SolutionImgs)
                    .AsQueryable();

                // 1) filter status (optional)
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var s = status.Trim();
                    query = query.Where(x => x.Status == s);
                }

                // 2) search keyword (ไม่สนตัวเล็ก/ใหญ่)
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var kw = q.Trim();
                    query = query.Where(x =>
                        EF.Functions.Collate((x.Name ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        EF.Functions.Collate((x.Description ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                    );
                }

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.DepartmentId,
                        x.Name,
                        x.Description,
                        x.Status,
                        x.CreatedAt,

                        // ✅ ดึงรูปปก (Cover) ก่อน ถ้าไม่มี cover ให้เอารูปแรกแทน
                        CoverUrl = x.SolutionImgs
                            .OrderByDescending(img => img.IsCover)   // cover จะมาก่อน
                            .ThenBy(img => img.Id)
                            .Select(img => string.IsNullOrEmpty(img.Image) ? null : $"{baseUrl}/{img.Image}")
                            .FirstOrDefault(),

                        ImagesCount = x.SolutionImgs.Count(),

                      
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
