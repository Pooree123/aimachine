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
    private readonly IWebHostEnvironment _environment;

    public CommentsController(AimachineContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: ดึงข้อมูลทั้งหมด (เฉพาะที่ Status = 'Active' เท่านั้น เพื่อนำไปโชว์)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Where(c => c.Status == "Active") // ✅ Logic: คัดเฉพาะที่เปิดการแสดงผล
          .OrderByDescending(c => c.Id)
          .Select(c => new
          {
              c.Id,
              c.JobTitleId,
              ProfileImg = !string.IsNullOrEmpty(c.ProfileImg) && c.ProfileImg.StartsWith("http")
                           ? c.ProfileImg
                           : (string.IsNullOrEmpty(c.ProfileImg) ? null : $"{baseUrl}/{c.ProfileImg}"),
              c.Name,
              c.Message,
              c.CreatedAt
          })
          .ToListAsync());
    }

    // GET: ดึงตาม JobTitle (เฉพาะที่ Status = 'Active')
    [HttpGet("by-jobtitle/{jobTitleId:int}")]
    public async Task<IActionResult> GetByJobTitle(int jobTitleId)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Where(c => c.JobTitleId == jobTitleId && c.Status == "Active") // ✅ Logic: คัดเฉพาะที่เปิดการแสดงผล
          .OrderByDescending(c => c.Id)
          .Select(c => new
          {
              c.Id,
              ProfileImg = !string.IsNullOrEmpty(c.ProfileImg) && c.ProfileImg.StartsWith("http")
                           ? c.ProfileImg
                           : (string.IsNullOrEmpty(c.ProfileImg) ? null : $"{baseUrl}/{c.ProfileImg}"),
              c.Name,
              c.Message,
              c.CreatedAt
          })
          .ToListAsync());
    }

    // POST: สร้างคอมเมนต์ใหม่ (Default Status = 'Active' คือโชว์เลย)
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCommentDto dto)
    {
        if (dto.JobTitleId <= 0) return BadRequest(new { Message = "job_title_id ไม่ถูกต้อง" });

        if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
            return BadRequest(new { Message = "ไม่พบ job_title นี้" });

        try
        {
            // 1. จัดการรูปภาพ (ใช้ Default ถ้าไม่มีการอัปโหลด)
            string imagePath = "uploads/Default_pfp.jpg";

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "comments");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
                string fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }
                imagePath = $"uploads/comments/{fileName}";
            }
            else if (!string.IsNullOrEmpty(dto.ProfileImg))
            {
                imagePath = dto.ProfileImg;
            }

            // 2. บันทึกข้อมูล
            var entity = new Comment
            {
                JobTitleId = dto.JobTitleId,
                ProfileImg = imagePath,
                Name = dto.Name?.Trim(),
                Message = dto.Message?.Trim(),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Status = "Active" // ✅ เริ่มต้นให้เป็น Active (โชว์ทันที)
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

    // DELETE: ซ่อนคอมเมนต์ (เปลี่ยน Status เป็น 'inActive')
    // การเรียก API Delete นี้ จะไม่ได้ลบข้อมูลหายไป แต่จะปิดไม่ให้แสดงผลบนหน้าเว็บ
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> HideComment(int id)
    {
        try
        {
            var entity = await _context.Comments.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบคอมเมนต์" });

            // ✅ Logic: เปลี่ยนเป็น inActive เพื่อปิดการแสดงผล (Hide)
            entity.Status = "inActive";

            _context.Comments.Update(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ซ่อนคอมเมนต์สำเร็จ (Status -> inActive)" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "ดำเนินการไม่สำเร็จ", Error = ex.Message });
        }
    }
}