using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/event-categories")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly AimachineContext _context;
        public EventCategoriesController(AimachineContext context) => _context = context;

        // GET: /api/event-categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EventCategories
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.EventTitle,
                    x.CreatedBy,
                    x.UpdateBy,
                    x.CreatedAt,
                    x.UpdateAt
                    // x.Status // ถ้ามี
                })
                .ToListAsync();

            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        // GET: /api/event-categories/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _context.EventCategories
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.EventTitle,
                    x.CreatedBy,
                    x.UpdateBy,
                    x.CreatedAt,
                    x.UpdateAt
                    // x.Status // ถ้ามี
                })
                .FirstOrDefaultAsync();

            if (data == null) return NotFound(new { Message = "ไม่พบ Category" });
            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        // POST: /api/event-categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            var title = dto.EventTitle.Trim();

            // กันชื่อซ้ำ (ถ้าต้องการ)
            var exists = await _context.EventCategories.AnyAsync(x => x.EventTitle == title);
            if (exists) return BadRequest(new { Message = "ชื่อ Category นี้มีอยู่แล้ว" });

            var now = DateTime.UtcNow.AddHours(7);

            var entity = new EventCategory
            {
                EventTitle = title,
                CreatedBy = dto.CreatedBy,
                UpdateBy = dto.CreatedBy,
                CreatedAt = now,
                UpdateAt = now
                // Status = dto.Status ?? "Active" // ถ้ามี
            };

            _context.EventCategories.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "เพิ่ม Category สำเร็จ", Id = entity.Id });
        }

        // PUT: /api/event-categories/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            var entity = await _context.EventCategories.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบ Category" });

            var title = dto.EventTitle.Trim();

            // กันชื่อซ้ำ (ยกเว้นตัวเอง)
            var dup = await _context.EventCategories.AnyAsync(x => x.Id != id && x.EventTitle == title);
            if (dup) return BadRequest(new { Message = "ชื่อ Category นี้มีอยู่แล้ว" });

            entity.EventTitle = title;
            entity.UpdateBy = dto.UpdateBy;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);
            // if (!string.IsNullOrWhiteSpace(dto.Status)) entity.Status = dto.Status; // ถ้ามี

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไข Category สำเร็จ" });
        }

        // DELETE: /api/event-categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.EventCategories.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบ Category" });

            // ✅ กันลบ ถ้ายังมี event อยู่ในหมวดนี้
            var hasEvents = await _context.Events.AnyAsync(e => e.CategoryId == id);
            if (hasEvents)
                return BadRequest(new { Message = "ลบไม่ได้: Category นี้มี Event อยู่" });

            _context.EventCategories.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบ Category สำเร็จ" });
        }
    }
}
