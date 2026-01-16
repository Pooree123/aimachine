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

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

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
    }
}