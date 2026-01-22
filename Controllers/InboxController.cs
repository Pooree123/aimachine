using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // ✅ GET: /api/inbox?topicId=1
        // ดึงเฉพาะที่ Deleteflag != true (คือยังไม่ถูกลบ)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? topicId)
        {
            try
            {
                var query = _context.Inboxes
                  .AsNoTracking()
                  .Where(x => x.Deleteflag != true) // ✅ Filter Soft Delete
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

        // ✅ GET: /api/inbox/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.Inboxes
                  .AsNoTracking()
                  .Include(x => x.Title)
                  // ✅ เช็คทั้ง ID และต้องยังไม่ถูกลบ
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

        // ✅ POST: /api/inbox
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
                    CreatedBy = request.CreatedBy,
                    UpdateBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdateAt = DateTime.UtcNow.AddHours(7),
                    Deleteflag = false // ✅ กำหนดค่าเริ่มต้น
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

        // ✅ PUT: /api/inbox/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInboxDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                var entity = await _context.Inboxes.FindAsync(id);
                // ต้องเช็คด้วยว่ารายการนี้ถูกลบไปแล้วหรือยัง
                if (entity == null || entity.Deleteflag == true)
                    return NotFound(new { Message = "ไม่พบข้อมูล Inbox นี้" });

                var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TitleId);
                if (!topicExists)
                    return BadRequest(new { Message = "Topic ไม่ถูกต้อง (ไม่พบในระบบ)" });

                entity.TitleId = request.TitleId;
                entity.Name = request.Name.Trim();
                entity.Message = request.Message.Trim();
                entity.Phone = request.Phone?.Trim();
                entity.Email = request.Email?.Trim();
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

        // ✅ DELETE: /api/inbox/5 (เปลี่ยนเป็น Soft Delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Inboxes.FindAsync(id);
                if (entity == null || entity.Deleteflag == true)
                    return NotFound(new { Message = "ไม่พบข้อมูล Inbox นี้" });

                // ✅ เปลี่ยน Logic เป็น Soft Delete
                entity.Deleteflag = true;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7); // อัปเดตเวลาลบด้วย

                // _context.Inboxes.Remove(entity); // ❌ ไม่ใช้ Remove แล้ว
                _context.Inboxes.Update(entity);    // ✅ ใช้ Update แทน

                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ (Soft Delete)" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }
    }
}