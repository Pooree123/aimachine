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

        public EventCategoriesController(AimachineContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/event-categories
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
                })
                .ToListAsync();

            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        // ✅ GET: /api/event-categories/5
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
                })
                .FirstOrDefaultAsync();

            if (data == null) return NotFound(new { Message = "ไม่พบ Category" });
            return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
        }

        // ✅ POST: /api/event-categories
        // body: { "eventTitle": "Christmasday", "createdBy": 1 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCategory body)
        {
            if (string.IsNullOrWhiteSpace(body.EventTitle))
                return BadRequest(new { Message = "EventTitle ห้ามว่าง" });

            var now = DateTime.UtcNow.AddHours(7);

            var entity = new EventCategory
            {
                EventTitle = body.EventTitle.Trim(),
                CreatedBy = body.CreatedBy,
                UpdateBy = body.CreatedBy,
                CreatedAt = now,
                UpdateAt = now
            };

            _context.EventCategories.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "เพิ่ม Category สำเร็จ", Id = entity.Id });
        }

        // ✅ PUT: /api/event-categories/5
        // body: { "eventTitle": "New Year", "updateBy": 1 }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventCategory body)
        {
            var entity = await _context.EventCategories.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบ Category" });

            if (string.IsNullOrWhiteSpace(body.EventTitle))
                return BadRequest(new { Message = "EventTitle ห้ามว่าง" });

            entity.EventTitle = body.EventTitle.Trim();
            entity.UpdateBy = body.UpdateBy;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไข Category สำเร็จ" });
        }

        // ✅ DELETE: /api/event-categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.EventCategories.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบ Category" });

            var used = await _context.Events.AnyAsync(e => e.CategoryId == id);
            if (used)
                return BadRequest(new { Message = "ลบไม่ได้: Category นี้ถูกใช้อยู่ใน Events" });

            _context.EventCategories.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบ Category สำเร็จ" });
        }

        // ✅ GET: /api/event-categories/search?q=chr
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q)
        {
            var query = _context.EventCategories.AsNoTracking().AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim();

                query = query.Where(x =>
                    EF.Functions.Collate(
                        (x.EventTitle ?? ""),
                        "SQL_Latin1_General_CP1_CI_AS"
                    ).Contains(kw)
                );
            }

            var data = await query
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.EventTitle,
                    x.CreatedBy,
                    x.UpdateBy,
                    x.CreatedAt,
                    x.UpdateAt
                })
                .ToListAsync();

            return Ok(new { Message = "ค้นหาสำเร็จ", Data = data });
        }


        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var data = await _context.EventCategories
                .AsNoTracking()
                .OrderBy(x => x.EventTitle)
                .Select(x => new
                {
                    x.Id,
                    x.EventTitle
                })
                .ToListAsync();

            return Ok(new
            {
                Message = "ดึงข้อมูล Category สำหรับ Dropdown สำเร็จ",
                Data = data
            });
        }

    }
}