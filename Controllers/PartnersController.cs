using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers
{
<<<<<<< HEAD
    [ApiController]
    [Route("api/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly AimachineContext _context;

        public PartnersController(AimachineContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/partners
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.Partners
                  .AsNoTracking()
                  .OrderByDescending(x => x.Id)
                  .Select(x => new
                  {
                      x.Id,
                      x.Image,
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
                var data = await _context.Partners
                  .AsNoTracking()
                  .Where(x => x.Id == id)
                  .Select(x => new
                  {
                      x.Id,
                      x.Image,
                      x.Name,
                      x.Status,
                      x.CreatedBy,
                      CreatedByName = x.CreatedByNavigation != null ? x.CreatedByNavigation.FullName : null,
                      x.UpdateBy,
                      UpdateByName = x.UpdateByNavigation != null ? x.UpdateByNavigation.FullName : null,
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
        public async Task<IActionResult> Create([FromBody] CreatePartnerDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                // เช็ค admin_users ถ้าส่ง createdBy มา
                if (request.CreatedBy.HasValue)
                {
                    var adminExists = await _context.AdminUsers.AnyAsync(a => a.Id == request.CreatedBy.Value);
                    if (!adminExists)
                        return BadRequest(new { Message = "created_by ไม่ถูกต้อง (ไม่พบ admin_users)" });
                }

                // กัน status
                if (!string.IsNullOrWhiteSpace(request.Status) &&
          request.Status != "Active" && request.Status != "inActive")
                {
                    return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive เท่านั้น" });
                }

                var entity = new Partner
                {
                    Image = request.Image?.Trim(),
                    Name = request.Name.Trim(),
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
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Message = "เพิ่มข้อมูลไม่สำเร็จ (DbUpdateException)",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ PUT: /api/partners/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePartnerDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            var entity = await _context.Partners.FindAsync(id);
            if (entity == null)
                return NotFound(new { Message = "ไม่พบ Partner นี้" });

            if (request.UpdateBy.HasValue &&
              !await _context.AdminUsers.AnyAsync(a => a.Id == request.UpdateBy.Value))
                return BadRequest(new { Message = "update_by ไม่ถูกต้อง" });

            if (!string.IsNullOrWhiteSpace(request.Status) &&
              request.Status != "Active" && request.Status != "inActive")
                return BadRequest(new { Message = "Status ต้องเป็น Active หรือ inActive" });

            // ✅ แก้ตรงนี้: อัปเดตรูปเฉพาะตอนมีค่าใหม่
            if (!string.IsNullOrWhiteSpace(request.Image))
                entity.Image = request.Image.Trim();

            entity.Name = request.Name.Trim();
            entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status;
            entity.UpdateBy = request.UpdateBy;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);
=======
    [Route("api/[controller]")]
    [ApiController]
    public class PartnersController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IWebHostEnvironment _environment; // ตัวช่วยหา path wwwroot

        public PartnersController(AimachineContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // สร้าง Base URL (เช่น http://localhost:5000)
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var partners = await _context.Partners
                .Where(p => p.Status != "Deleted") 
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Status,
                    ImageUrl = string.IsNullOrEmpty(p.Image)
                        ? null
                        : $"{baseUrl}/images/partners/{p.Image}"
                })
                .ToListAsync();

            return Ok(partners);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePartnerDto request)
        {
            string newFileName = null;

            if (request.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "partners");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                newFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, newFileName);


                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(fileStream);
                }
            }

            // 4. บันทึกลง Database
            var partner = new Partner
            {
                Name = request.Name,
                Image = newFileName, // เก็บแค่ชื่อไฟล์ใน DB (เช่น abc-123.jpg)
                Status = request.Status,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdateAt = DateTime.UtcNow.AddHours(7)
            };

            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "สร้างสำเร็จ", Data = partner });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdatePartnerDto request)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            if (request.ImageFile != null)

            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "partners");

                if (!string.IsNullOrEmpty(partner.Image))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, partner.Image);
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, newFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(fileStream);
                }

                partner.Image = newFileName;
            }

            partner.Name = request.Name;
            partner.Status = request.Status;
            partner.UpdateAt = DateTime.UtcNow.AddHours(7);
>>>>>>> origin/main

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

<<<<<<< HEAD

        // ✅ DELETE: /api/partners/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Partners.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Partner นี้" });

                _context.Partners.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Message = "ลบข้อมูลไม่สำเร็จ (อาจติด constraint หรือมีข้อมูลอ้างอิงอยู่)",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "กรุณาเลือกไฟล์" });

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
            if (!allowed.Contains(ext))
                return BadRequest(new { Message = "รองรับเฉพาะ png/jpg/jpeg/webp" });

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "partners");
            Directory.CreateDirectory(folder);

            var newFileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, newFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"uploads/partners/{newFileName}";
            return Ok(new { Message = "อัปโหลดสำเร็จ", Path = relativePath });
        }


=======
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            if (!string.IsNullOrEmpty(partner.Image))
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "partners");
                string filePath = Path.Combine(uploadsFolder, partner.Image);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
        }
>>>>>>> origin/main
    }
}