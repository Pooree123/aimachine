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
    public CommentsController(AimachineContext context) => _context = context;

    // GET: all
    [HttpGet]
    public async Task<IActionResult> GetAll()
    => Ok(await _context.Comments.AsNoTracking()
      .OrderByDescending(c => c.Id)
      .Select(c => new {
          c.Id,
          c.JobTitleId,
          c.ProfileImg,
          c.Name,
          c.Message,
          c.CreatedAt
      })
      .ToListAsync());

    // GET: by job title (one -> many)
    [HttpGet("by-jobtitle/{jobTitleId:int}")]
    public async Task<IActionResult> GetByJobTitle(int jobTitleId)
    => Ok(await _context.Comments.AsNoTracking()
      .Where(c => c.JobTitleId == jobTitleId)
      .OrderByDescending(c => c.Id)
      .Select(c => new {
          c.Id,
          c.ProfileImg,
          c.Name,
          c.Message,
          c.CreatedAt
      })
      .ToListAsync());

    // POST
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto)
    {
        if (dto.JobTitleId <= 0)
            return BadRequest(new { Message = "job_title_id ไม่ถูกต้อง" });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { Message = "กรุณากรอก name" });

        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { Message = "กรุณากรอก message" });

        if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
            return BadRequest(new { Message = "ไม่พบ job_title นี้" });

        var entity = new Comment
        {
            JobTitleId = dto.JobTitleId,
            ProfileImg = string.IsNullOrWhiteSpace(dto.ProfileImg)
            ? "uploads/Default_pfp.jpg"   // ✅ default
                    : dto.ProfileImg.Trim(),
            Name = dto.Name.Trim(),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        _context.Comments.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "เพิ่มคอมเมนต์สำเร็จ", Id = entity.Id });
    }


    // DELETE (ลบจริง)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Comments.FindAsync(id);
        if (entity == null)
            return NotFound(new { Message = "ไม่พบคอมเมนต์" });

        _context.Comments.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "ลบคอมเมนต์สำเร็จ" });
    }
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "ไม่มีไฟล์" });

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/comments");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Ok(new { Path = $"uploads/comments/{fileName}" });
    }

}
