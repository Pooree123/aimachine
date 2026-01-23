using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // ✅ GET: /api/partners
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                var data = await _context.Partners
                  .AsNoTracking()
                  .Where(x => x.Status != "Deleted")
                  .OrderByDescending(x => x.Id)
                  .Select(x => new
                  {
                      x.Id,
                      // ✅ 1. แก้ URL ตอนดึงรูปมาแสดง (จาก images -> uploads)
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

        // ✅ GET: /api/partners/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                var data = await _context.Partners
                  .AsNoTracking()
                  .Where(x => x.Id == id)
                  .Select(x => new
                  {
                      x.Id,
                      // ✅ 2. แก้ URL ตรงนี้ด้วย
                      ImageUrl = string.IsNullOrEmpty(x.Image) ? null : $"{baseUrl}/uploads/partners/{x.Image}",
                      x.Name,
                      x.Status,
                      x.CreatedBy,
                      CreatedByName = x.CreatedByNavigation != null ? x.CreatedByNavigation.FullName : null,
                      x.CreatedAt,
                      x.UpdateAt
                  })
                  .FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new { Message = "ไม่พบ Partner นี้" });

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ POST: /api/partners
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePartnerDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                if (request.CreatedBy.HasValue)
                {
                    var adminExists = await _context.AdminUsers.AnyAsync(a => a.Id == request.CreatedBy.Value);
                    if (!adminExists) return BadRequest(new { Message = "created_by ไม่ถูกต้อง (ไม่พบ admin_users)" });
                }

                if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "Active" && request.Status != "inActive")
                {
                    return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive เท่านั้น" });
                }

                string newFileName = null;
                if (request.ImageFile != null)
                {
                    // ✅ 3. แก้ Path ที่บันทึกไฟล์ (images -> uploads)
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "partners");

                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    newFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, newFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var entity = new Partner
                {
                    Name = request.Name.Trim(),
                    Image = newFileName,
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
                    CreatedBy = request.CreatedBy,
                    UpdateBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdateAt = DateTime.UtcNow.AddHours(7)
                };

                _context.Partners.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ PUT: /api/partners/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePartnerDto request)
        {
            try
            {
                var entity = await _context.Partners.FindAsync(id);
                if (entity == null) return NotFound(new { Message = "ไม่พบ Partner นี้" });

                if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "Active" && request.Status != "inActive")
                    return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

                if (request.ImageFile != null)
                {
                    // ✅ 4. แก้ Path ตอนแก้ไขไฟล์ (images -> uploads)
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "partners");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // ลบไฟล์เก่า
                    if (!string.IsNullOrEmpty(entity.Image))
                    {
                        string oldFilePath = Path.Combine(uploadsFolder, entity.Image);
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }

                    // ลงไฟล์ใหม่
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, newFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ImageFile.CopyToAsync(fileStream);
                    }

                    entity.Image = newFileName;
                }

                entity.Name = request.Name.Trim();
                if (!string.IsNullOrWhiteSpace(request.Status)) entity.Status = request.Status;

                entity.UpdateBy = request.UpdateBy;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();
                return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ DELETE: /api/partners/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Partners.FindAsync(id);
                if (entity == null) return NotFound(new { Message = "ไม่พบ Partner นี้" });

                if (!string.IsNullOrEmpty(entity.Image))
                {
                    // ✅ 5. แก้ Path ตอนลบไฟล์ (images -> uploads)
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "partners");
                    string filePath = Path.Combine(uploadsFolder, entity.Image);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Partners.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
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
                    .AsQueryable();

                // ✅ ค้นหาจากชื่อ (ไม่สนตัวเล็ก/ใหญ่)
                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();
                    query = query.Where(p =>
                        EF.Functions.Collate((p.Name ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                    );
                }

                var data = await query
                    .OrderByDescending(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        ImageUrl = string.IsNullOrEmpty(p.Image)
                            ? null
                            : $"{baseUrl}/uploads/partners/{p.Image}",
                        p.Name,
                        p.Status,     // ส่งไปให้ FE แสดงป้าย Active/inActive ได้
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