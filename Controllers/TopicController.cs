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
    public class TopicController : ControllerBase
    {
        private readonly AimachineContext _context;

        public TopicController(AimachineContext context)
        {
            _context = context;
        }

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
                      // ดึงชื่อคนสร้างมาแสดง (ถ้ามี)
                      CreatedByName = t.CreatedByNavigation != null ? t.CreatedByNavigation.FullName : "",
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateTopicDto request)
        {
            // ✅ ใช้ ID จาก Token เสมอ
            int currentUserId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                var title = request.TopicTitle.Trim();

                // เช็คชื่อซ้ำ
                var exists = await _context.Topics.AnyAsync(t => t.TopicTitle == title);
                if (exists)
                    return BadRequest(new { Message = "มี Topic ชื่อนี้อยู่แล้ว" });

                var entity = new Topic
                {
                    TopicTitle = title,
                    CreatedBy = currentUserId, // ใส่ ID คนสร้าง
                    UpdateBy = currentUserId,  // ใส่ ID คนแก้ (ครั้งแรกคือคนเดียวกัน)
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdateAt = DateTime.UtcNow.AddHours(7)
                };

                _context.Topics.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTopicDto request)
        {
            // ✅ ใช้ ID จาก Token เสมอ
            int currentUserId = User.GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            try
            {
                var entity = await _context.Topics.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Topic นี้" });

                var title = request.TopicTitle.Trim();

                // เช็คชื่อซ้ำ (ไม่นับตัวเอง)
                var duplicate = await _context.Topics.AnyAsync(t => t.Id != id && t.TopicTitle == title);
                if (duplicate)
                    return BadRequest(new { Message = "ชื่อ Topic ซ้ำกับรายการอื่น" });

                entity.TopicTitle = title;
                entity.UpdateBy = currentUserId; // อัปเดตคนแก้ไขล่าสุด
                entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();

                return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
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

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] TopicSearchQueryDto req)
        {
            try
            {
                var query = _context.Topics
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();
                    // ใช้ Contains ธรรมดา เพื่อความชัวร์และปลอดภัย
                    query = query.Where(x => x.TopicTitle != null && x.TopicTitle.Contains(kw));
                }

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.TopicTitle,
                        x.CreatedBy,
                        x.UpdateBy,
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

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            try
            {
                var data = await _context.Topics
                    .AsNoTracking()
                    .OrderBy(t => t.TopicTitle)
                    .Select(t => new TopicDropdownDto
                    {
                        Value = t.Id,
                        Label = t.TopicTitle ?? ""
                    })
                    .ToListAsync();

                return Ok(new { Message = "โหลด dropdown สำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "โหลด dropdown ไม่สำเร็จ", Error = ex.Message });
            }
        }
    }
}