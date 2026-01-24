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
    public class PartnersController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IWebHostEnvironment _environment;

        public PartnersController(AimachineContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

        [HttpGet]
        public async Task<IActionResult> GetPublicPartners()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var data = await _context.DepartmentTypes
                .AsNoTracking()
                .Where(d => d.Partners.Any(p => p.Status == "Active"))
                .Select(d => new
                {
                    DepartmentId = d.Id,
                    DepartmentTitle = d.DepartmentTitle,
                    Partners = d.Partners
                        .Where(p => p.Status == "Active")
                        .OrderByDescending(p => p.Id)
                        .Select(p => new
                        {
                            p.Id,
                            p.Name,
                            p.Status,
                            ImageUrl = string.IsNullOrEmpty(p.Image) ? null : $"{baseUrl}/uploads/partners/{p.Image}",
                            p.CreatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("admin")]
        [Authorize]
        public async Task<IActionResult> GetAdminPartners()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                var data = await _context.Partners
                  .AsNoTracking()
                  .Include(x => x.Department)
                  .Include(x => x.CreatedByNavigation)
                  .Include(x => x.UpdateByNavigation)
                  .Where(x => x.Status != "Deleted")
                  .OrderByDescending(x => x.Id)
                  .Select(x => new
                  {
                      x.Id,
                      x.DepartmentId,
                      DepartmentTitle = x.Department != null ? x.Department.DepartmentTitle : "",
                      ImageUrl = string.IsNullOrEmpty(x.Image) ? null : $"{baseUrl}/uploads/partners/{x.Image}",
                      x.Name,
                      x.Status,
                      x.CreatedBy,
                      CreatedByName = x.CreatedByNavigation != null ? x.CreatedByNavigation.FullName : null,
                      x.UpdateBy,
                      UpdateByName = x.UpdateByNavigation != null ? x.UpdateByNavigation.FullName : null,
                      x.CreatedAt,
                      x.UpdateAt
                  })
                  .ToListAsync();

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                var data = await _context.Partners
                  .AsNoTracking()
                  .Include(x => x.Department)
                  .Include(x => x.CreatedByNavigation)
                  .Where(x => x.Id == id)
                  .Select(x => new
                  {
                      x.Id,
                      x.DepartmentId,
                      DepartmentTitle = x.Department != null ? x.Department.DepartmentTitle : "",
                      ImageUrl = string.IsNullOrEmpty(x.Image) ? null : $"{baseUrl}/uploads/partners/{x.Image}",
                      x.Name,
                      x.Status,
                      x.CreatedBy,
                      CreatedByName = x.CreatedByNavigation != null ? x.CreatedByNavigation.FullName : null,
                      x.CreatedAt,
                      x.UpdateAt
                  })
                  .FirstOrDefaultAsync();

                if (data == null) return NotFound(new { Message = "ไม่พบ Partner นี้" });

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreatePartnerDto request)
        {
            int currentUserId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            // 🛡️ ตรวจสอบไฟล์รูปภาพ (ขนาด + ประเภท)
            if (request.ImageFile != null && !IsAllowedImageFile(request.ImageFile))
            {
                return BadRequest(new { Message = $"ไฟล์ '{request.ImageFile.FileName}' ไม่ถูกต้อง (ต้องเป็น .jpg/.png และขนาดไม่เกิน 5MB)" });
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (request.DepartmentId.HasValue)
                    {
                        if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == request.DepartmentId))
                            return BadRequest(new { Message = "ไม่พบ Department ID นี้ในระบบ" });
                    }

                    if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "Active" && request.Status != "inActive")
                        return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive เท่านั้น" });

                    var entity = new Partner
                    {
                        Name = request.Name.Trim(),
                        DepartmentId = request.DepartmentId,
                        Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
                        CreatedBy = currentUserId,
                        UpdateBy = currentUserId,
                        CreatedAt = DateTime.UtcNow.AddHours(7),
                        UpdateAt = DateTime.UtcNow.AddHours(7)
                    };

                    _context.Partners.Add(entity);
                    await _context.SaveChangesAsync();

                    if (request.ImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "partners");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";
                        string filePath = Path.Combine(uploadsFolder, newFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await request.ImageFile.CopyToAsync(fileStream);
                        }

                        entity.Image = newFileName;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.InnerException?.Message ?? ex.Message });
                }
            });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePartnerDto request)
        {
            int currentUserId = User.GetUserId();

            // 🛡️ ตรวจสอบไฟล์รูปภาพใหม่ (ขนาด + ประเภท)
            if (request.ImageFile != null && !IsAllowedImageFile(request.ImageFile))
            {
                return BadRequest(new { Message = $"ไฟล์ '{request.ImageFile.FileName}' ไม่ถูกต้อง (ต้องเป็น .jpg/.png และขนาดไม่เกิน 5MB)" });
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var entity = await _context.Partners.FindAsync(id);
                    if (entity == null) return NotFound(new { Message = "ไม่พบ Partner นี้" });

                    if (request.DepartmentId.HasValue && request.DepartmentId != entity.DepartmentId)
                    {
                        if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == request.DepartmentId))
                            return BadRequest(new { Message = "ไม่พบ Department ID นี้ในระบบ" });
                    }

                    if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "Active" && request.Status != "inActive")
                        return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

                    entity.Name = request.Name.Trim();
                    entity.DepartmentId = request.DepartmentId;
                    if (!string.IsNullOrWhiteSpace(request.Status)) entity.Status = request.Status;
                    entity.UpdateBy = currentUserId;
                    entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                    if (request.ImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "partners");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        if (!string.IsNullOrEmpty(entity.Image))
                        {
                            string oldFilePath = Path.Combine(uploadsFolder, entity.Image);
                            if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        }

                        string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";
                        string filePath = Path.Combine(uploadsFolder, newFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await request.ImageFile.CopyToAsync(fileStream);
                        }

                        entity.Image = newFileName;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.InnerException?.Message ?? ex.Message });
                }
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Partners.FindAsync(id);
                if (entity == null) return NotFound(new { Message = "ไม่พบ Partner นี้" });

                string imageToDelete = entity.Image;

                _context.Partners.Remove(entity);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(imageToDelete))
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "partners");
                    string filePath = Path.Combine(uploadsFolder, imageToDelete);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Message = "ไม่สามารถลบได้ เนื่องจากข้อมูลนี้ถูกใช้งานอยู่ในส่วนอื่นของระบบ",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] PartnerSearchQueryDto req)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                var query = _context.Partners
                    .AsNoTracking()
                    .Include(p => p.Department)
                    .AsQueryable();

                if (req.DepartmentId.HasValue)
                {
                    query = query.Where(p => p.DepartmentId == req.DepartmentId.Value);
                }

                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();
                    query = query.Where(p =>
                        p.Name != null && p.Name.Contains(kw)
                    );
                }

                var data = await query
                    .OrderByDescending(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.DepartmentId,
                        DepartmentTitle = p.Department != null ? p.Department.DepartmentTitle : "",
                        ImageUrl = string.IsNullOrEmpty(p.Image)
                            ? null
                            : $"{baseUrl}/uploads/partners/{p.Image}",
                        p.Name,
                        p.Status,
                        p.CreatedAt,
                        p.UpdateAt
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