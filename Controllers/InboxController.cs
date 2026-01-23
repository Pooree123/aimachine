using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InboxController : ControllerBase
    {
        private readonly AimachineContext _context;

        public InboxController(AimachineContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int? topicId)
        {
            try
            {
                var query = _context.Inboxes
                  .AsNoTracking()
                  .Where(x => x.Deleteflag != true)
                  .Include(x => x.Title)
                  .AsQueryable();

                if (topicId.HasValue)
                    query = query.Where(x => x.TitleId == topicId.Value);

                var data = await query
                  .OrderByDescending(x => x.CreatedAt)
                  .Select(x => new
                  {
                      x.Id,
                      x.TitleId,
                      TopicTitle = x.Title != null ? x.Title.TopicTitle : null,
                      x.Name,
                      x.Message,
                      x.Phone,
                      x.Email,
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
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.Inboxes
                  .AsNoTracking()
                  .Include(x => x.Title)
   
                  .Where(x => x.Id == id && x.Deleteflag != true)
                  .Select(x => new
                  {
                      x.Id,
                      x.TitleId,
                      TopicTitle = x.Title != null ? x.Title.TopicTitle : null,
                      x.Name,
                      x.Message,
                      x.Phone,
                      x.Email,
                      x.CreatedAt,
                      x.UpdateAt
                  })
                  .FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new { Message = "ไม่พบข้อมูล Inbox นี้" });

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInboxDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TitleId);
                if (!topicExists)
                    return BadRequest(new { Message = "Topic ไม่ถูกต้อง (ไม่พบในระบบ)" });

                var entity = new Inbox
                {
                    TitleId = request.TitleId,
                    Name = request.Name.Trim(),
                    Message = request.Message.Trim(),
                    Phone = request.Phone?.Trim(),
                    Email = request.Email?.Trim(),
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdateAt = DateTime.UtcNow.AddHours(7),
                    Deleteflag = false 
                };

                _context.Inboxes.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Inboxes.FindAsync(id);
                if (entity == null || entity.Deleteflag == true)
                    return NotFound(new { Message = "ไม่พบข้อมูล Inbox นี้" });

                entity.Deleteflag = true;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7); 


                _context.Inboxes.Update(entity);    

                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ (Soft Delete)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] InboxSearchQueryDto req)
        {
            try
            {
                var query = _context.Inboxes
                    .AsNoTracking()
                    .Where(x => x.Deleteflag != true)
                    .Include(x => x.Title)
                    .AsQueryable();

                // 1) Filter topic (optional)
                if (req.TopicId.HasValue)
                    query = query.Where(x => x.TitleId == req.TopicId.Value);

                // 2) Filter date (optional) - เทียบเฉพาะวัน
                if (req.Date.HasValue)
                {
                    var d = req.Date.Value.Date;
                    var next = d.AddDays(1);
                    query = query.Where(x => x.CreatedAt >= d && x.CreatedAt < next);
                }

                // 3) Search keyword (optional) - ไม่สนตัวเล็ก/ใหญ่
                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();

                    query = query.Where(x =>
                        EF.Functions.Collate((x.Name ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        EF.Functions.Collate((x.Email ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        EF.Functions.Collate((x.Phone ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        EF.Functions.Collate((x.Message ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        (x.Title != null && EF.Functions.Collate((x.Title.TopicTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw))
                    );
                }

                var data = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.TitleId,
                        TopicTitle = x.Title != null ? x.Title.TopicTitle : null,
                        x.Name,
                        x.Message,
                        x.Phone,
                        x.Email,
                        x.CreatedAt,
                        x.UpdateAt
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