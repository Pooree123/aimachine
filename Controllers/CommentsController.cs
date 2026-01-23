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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Where(c => c.Status == "Deleted") 
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

    [HttpGet("by-jobtitle/{jobTitleId:int}")]
    public async Task<IActionResult> GetByJobTitle(int jobTitleId)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Where(c => c.JobTitleId == jobTitleId && c.Status == "Active") 
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

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCommentDto dto)
    {
        if (dto.JobTitleId <= 0) return BadRequest(new { Message = "job_title_id ไม่ถูกต้อง" });

        if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
            return BadRequest(new { Message = "ไม่พบ job_title นี้" });

        try
        {
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

            var entity = new Comment
            {
                JobTitleId = dto.JobTitleId,
                ProfileImg = imagePath,
                Name = dto.Name?.Trim(),
                Message = dto.Message?.Trim(),
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Status = "Active" 
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> HideComment(int id)
    {
        try
        {
            var entity = await _context.Comments.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบคอมเมนต์" });

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

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] CommentSearchQueryDto req)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var query = _context.Comments
                .AsNoTracking()
                .AsQueryable();

            // 1) Filter jobTitleId (optional)
            if (req.JobTitleId.HasValue)
                query = query.Where(c => c.JobTitleId == req.JobTitleId.Value);

            // 2) Filter date (optional) เทียบเฉพาะวัน
            if (req.Date.HasValue)
            {
                var d = req.Date.Value.Date;
                var next = d.AddDays(1);
                query = query.Where(c => c.CreatedAt >= d && c.CreatedAt < next);
            }

            // 3) Search keyword (optional) - ไม่สนตัวเล็ก/ใหญ่
            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var kw = req.Q.Trim();
                query = query.Where(c =>
                    EF.Functions.Collate((c.Name ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                    EF.Functions.Collate((c.Message ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                );
            }

            var data = await query
                .OrderByDescending(c => c.Id)
                .Select(c => new
                {
                    c.Id,
                    c.JobTitleId,

                    ProfileImg = !string.IsNullOrEmpty(c.ProfileImg) && c.ProfileImg.StartsWith("http")
                        ? c.ProfileImg
                        : (string.IsNullOrEmpty(c.ProfileImg) ? null : $"{baseUrl}/{c.ProfileImg}"),

                    Name = c.Name,
                    Message = c.Message,

                    c.Status,
                    c.CreatedAt,
                    
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