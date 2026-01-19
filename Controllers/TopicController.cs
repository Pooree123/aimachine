using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly AimachineContext _context;

        public TopicController(AimachineContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/topic
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.Topics
                  .AsNoTracking()
                  .OrderByDescending(t => t.Id)
                  .Select(t => new
                  {
                      t.Id,
                      t.TopicTitle,
                      t.CreatedBy,
                      t.UpdateBy,
                      t.CreatedAt,
                      t.UpdateAt
                  })
                  .ToListAsync();

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ GET: /api/topic/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _context.Topics
                  .AsNoTracking()
                  .Where(t => t.Id == id)
                  .Select(t => new
                  {
                      t.Id,
                      t.TopicTitle,
                      t.CreatedBy,
                      t.UpdateBy,
                      t.CreatedAt,
                      t.UpdateAt
                  })
                  .FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new { Message = "ไม่พบ Topic นี้" });

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ POST: /api/topic
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTopicDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                // 1. ตรวจสอบว่า Admin ที่สร้าง มีอยู่จริงหรือไม่
                if (request.CreatedBy.HasValue)
                {
                    var adminExists = await _context.AdminUsers.AnyAsync(a => a.Id == request.CreatedBy.Value);
                    if (!adminExists) return BadRequest(new { Message = "created_by ไม่ถูกต้อง (ไม่พบ admin_users)" });
                }

                var title = request.TopicTitle.Trim();

                // 2. เช็คชื่อซ้ำ (ป้องกัน Topic ชื่อเดียวกัน)
                var exists = await _context.Topics.AnyAsync(t => t.TopicTitle == title);
                if (exists)
                    return BadRequest(new { Message = "มี Topic ชื่อนี้อยู่แล้ว" });

                var entity = new Topic
                {
                    TopicTitle = title,
                    CreatedBy = request.CreatedBy,
                    UpdateBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdateAt = DateTime.UtcNow.AddHours(7)
                };

                _context.Topics.Add(entity);
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

        // ✅ PUT: /api/topic/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTopicDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                var entity = await _context.Topics.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Topic นี้" });

                // 1. ตรวจสอบว่า Admin ที่แก้ไข มีอยู่จริงหรือไม่
                if (request.UpdateBy.HasValue)
                {
                    var adminExists = await _context.AdminUsers.AnyAsync(a => a.Id == request.UpdateBy.Value);
                    if (!adminExists) return BadRequest(new { Message = "update_by ไม่ถูกต้อง" });
                }

                var title = request.TopicTitle.Trim();

                // 2. เช็คชื่อซ้ำ (ต้องไม่ซ้ำกับ ID อื่น)
                var duplicate = await _context.Topics.AnyAsync(t => t.Id != id && t.TopicTitle == title);
                if (duplicate)
                    return BadRequest(new { Message = "ชื่อ Topic ซ้ำกับรายการอื่น" });

                entity.TopicTitle = title;
                entity.UpdateBy = request.UpdateBy;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();

                return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Message = "แก้ไขข้อมูลไม่สำเร็จ (DbUpdateException)",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ DELETE: /api/topic/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Topics.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Topic นี้" });

                _context.Topics.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (DbUpdateException ex)
            {
                // ดักจับ Error กรณีลบไม่ได้เพราะถูกตารางอื่น (เช่น Inbox) ใช้งานอยู่
                return BadRequest(new
                {
                    Message = "ลบข้อมูลไม่สำเร็จ (เนื่องจาก Topic นี้ถูกใช้งานอยู่ในระบบ)",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }
    }
}