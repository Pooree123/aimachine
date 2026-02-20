using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;
using Aimachine.Services; // ✅ เพิ่มเพื่อใช้งาน Cloudinary

namespace Aimachine.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly AimachineContext _context;
    private readonly IPhotoService _photoService; // ✅ เปลี่ยนมาใช้ IPhotoService แทน

    public CommentsController(AimachineContext context, IPhotoService photoService)
    {
        _context = context;
        _photoService = photoService;
    }

    // ✅ Helper Function: เช็คไฟล์รูป
    private bool IsAllowedImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > 5 * 1024 * 1024) return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(ext);
    }

    // ✅ 1. Public: ดึงเฉพาะ Active (สำหรับหน้าบ้าน)
    [HttpGet]
    public async Task<IActionResult> GetPublicComments()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Where(c => c.Status == "Active") // 👈 กรองเฉพาะ Active
          .OrderByDescending(c => c.Id)
          .Select(c => new
          {
              c.Id,
              c.JobTitleId,
              JobTitleName = c.JobTitle != null ? c.JobTitle.JobsTitle : "",
              c.Status,
              // ✅ โค้ดคุณเขียนดักลิงก์ http ไว้ดีแล้ว รองรับ Cloudinary ได้ทันที
              ProfileImg = !string.IsNullOrEmpty(c.ProfileImg) && c.ProfileImg.StartsWith("http")
                           ? c.ProfileImg
                           : (string.IsNullOrEmpty(c.ProfileImg) ? null : $"{baseUrl}/{c.ProfileImg}"),
              c.Name,
              c.Message,
              c.CreatedAt
          })
          .ToListAsync());
    }

    // ✅ 2. Admin: ดึงทั้งหมด (Active + InActive)
    [HttpGet("admin")]
    [Authorize]
    public async Task<IActionResult> GetAdminComments()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        return Ok(await _context.Comments.AsNoTracking()
          .Include(c => c.JobTitle)
          .OrderByDescending(c => c.Id)
          .Select(c => new
          {
              c.Id,
              c.JobTitleId,
              JobTitleName = c.JobTitle != null ? c.JobTitle.JobsTitle : "",
              c.Status,
              ProfileImg = !string.IsNullOrEmpty(c.ProfileImg) && c.ProfileImg.StartsWith("http")
                           ? c.ProfileImg
                           : (string.IsNullOrEmpty(c.ProfileImg) ? null : $"{baseUrl}/{c.ProfileImg}"),
              c.Name,
              c.Message,
              c.CreatedAt
          })
          .ToListAsync());
    }

    // ✅ 3. Get By JobTitle (Public - Active Only)
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

    // ✅ 4. Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCommentDto dto)
    {
        if (dto.JobTitleId <= 0) return BadRequest(new { Message = "JobTitleId ไม่ถูกต้อง" });

        if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
            return BadRequest(new { Message = "ไม่พบตำแหน่งงานนี้" });

        if (dto.ImageFile != null && !IsAllowedImageFile(dto.ImageFile))
            return BadRequest(new { Message = "ไฟล์รูปภาพไม่ถูกต้อง (รองรับ .jpg, .png, .webp ขนาดไม่เกิน 5MB)" });

        try
        {
            // ใช้ค่า Default สำหรับกรณีที่ไม่ได้แนบรูปมา
            string imagePath = "uploads/Default_pfp.jpg";

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // ✅ อัปโหลดรูปขึ้น Cloudinary
                var uploadResult = await _photoService.AddPhotoAsync(dto.ImageFile, "comments");
                if (uploadResult.Error != null) throw new Exception(uploadResult.Error.Message);

                // ได้ URL เต็มมาเก็บลงฐานข้อมูลเลย
                imagePath = uploadResult.SecureUrl.AbsoluteUri;
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

            return Ok(new { Message = "เพิ่มคอมเมนต์สำเร็จ", Id = entity.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "เพิ่มคอมเมนต์ไม่สำเร็จ", Error = ex.Message });
        }
    }

    // ✅ 5. Update (PUT): แก้ไขเฉพาะ Status เท่านั้น
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateCommentStatusDto dto)
    {
        if (dto.Status != "Active" && dto.Status != "inActive")
        {
            return BadRequest(new { Message = "Status ต้องเป็น 'Active' หรือ 'inActive' เท่านั้น" });
        }

        var entity = await _context.Comments.FindAsync(id);
        if (entity == null) return NotFound(new { Message = "ไม่พบคอมเมนต์" });

        try
        {
            entity.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { Message = $"อัปเดตสถานะเป็น {dto.Status} สำเร็จ" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "อัปเดตสถานะไม่สำเร็จ", Error = ex.Message });
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

            if (req.JobTitleId.HasValue)
                query = query.Where(c => c.JobTitleId == req.JobTitleId.Value);

            if (req.Date.HasValue)
            {
                var d = req.Date.Value.Date;
                var next = d.AddDays(1);
                query = query.Where(c => c.CreatedAt >= d && c.CreatedAt < next);
            }

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
                    c.Name,
                    c.Message,
                    c.Status,
                    c.CreatedAt
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