using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly AimachineContext _context;
    private readonly IWebHostEnvironment _environment; // ✅ เพิ่มตัวจัดการ Path

    public CommentsController(AimachineContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: all
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // สร้าง Base URL
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .OrderByDescending(c => c.Id)
          .Select(c => new
          {
              c.Id,
              c.JobTitleId,
              // ✅ เช็คถ้าเป็น path เต็มอยู่แล้ว หรือเป็น path ภายใน
              ProfileImg = c.ProfileImg.StartsWith("http") ? c.ProfileImg : $"{baseUrl}/{c.ProfileImg}",
              c.Name,
              c.Message,
              c.CreatedAt
          })
          .ToListAsync());
    }

    // GET: by job title
    [HttpGet("by-jobtitle/{jobTitleId:int}")]
    public async Task<IActionResult> GetByJobTitle(int jobTitleId)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Where(c => c.JobTitleId == jobTitleId)
          .OrderByDescending(c => c.Id)
          .Select(c => new
          {
              c.Id,
              ProfileImg = c.ProfileImg.StartsWith("http") ? c.ProfileImg : $"{baseUrl}/{c.ProfileImg}",
              c.Name,
              c.Message,
              c.CreatedAt
          })
          .ToListAsync());
    }

    // ✅ POST (แก้ใหม่รองรับ Upload ในตัว)
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCommentDto dto) // เปลี่ยนเป็น FromForm
    {
        if (dto.JobTitleId <= 0) return BadRequest(new { Message = "job_title_id ไม่ถูกต้อง" });
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest(new { Message = "กรุณากรอก name" });
        if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest(new { Message = "กรุณากรอก message" });

        if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
            return BadRequest(new { Message = "ไม่พบ job_title นี้" });

        try
        {
            // 1. กำหนดค่าเริ่มต้นเป็นภาพ Default
            string imagePath = "uploads/Default_pfp.jpg";

            // 2. ถ้ามีการอัปโหลดไฟล์มา ให้บันทึกไฟล์
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // กำหนดโฟลเดอร์ปลายทาง
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "comments");

                // ถ้าไม่มีโฟลเดอร์ ให้สร้างใหม่
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // ตั้งชื่อไฟล์ใหม่กันซ้ำ
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
                string fullPath = Path.Combine(uploadsFolder, fileName);

                // บันทึกไฟล์ลงเครื่อง
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                // อัปเดต path ที่จะลง Database
                imagePath = $"uploads/comments/{fileName}";
            }

            // 3. บันทึกลง Database
            var entity = new Comment
            {
                JobTitleId = dto.JobTitleId,
                ProfileImg = imagePath, // ใช้ path ที่ได้จากการอัปโหลด หรือ Default
                Name = dto.Name.Trim(),
                Message = dto.Message.Trim(),
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            _context.Comments.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "เพิ่มคอมเมนต์สำเร็จ", Id = entity.Id, Image = imagePath });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "เพิ่มคอมเมนต์ไม่สำเร็จ", Error = ex.Message });
        }
    }

    // DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var entity = await _context.Comments.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบคอมเมนต์" });

            // (Option) ลบรูปออกจากโฟลเดอร์ด้วย ถ้าไม่ใช่รูป Default
            if (!string.IsNullOrEmpty(entity.ProfileImg) && !entity.ProfileImg.Contains("Default_pfp.jpg"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, entity.ProfileImg);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Comments.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบคอมเมนต์สำเร็จ" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "ลบไม่สำเร็จ", Error = ex.Message });
        }
    }
}