using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aimachine.DTOs;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

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
                    CreatedByName = x.CreatedByNavigation != null ? x.CreatedByNavigation.FullName : "",
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateEventCategoryDto body)
        {
            int currentUserId = User.GetUserId(); // ✅ ใช้ ID จาก Token

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var now = DateTime.UtcNow.AddHours(7);

                var entity = new EventCategory
                {
                    EventTitle = body.EventTitle.Trim(),
                    CreatedBy = currentUserId,
                    UpdateBy = currentUserId,
                    CreatedAt = now,
                    UpdateAt = now
                };

                _context.EventCategories.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "เพิ่ม Category สำเร็จ",
                    Id = entity.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "เพิ่มข้อมูลไม่สำเร็จ",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventCategoryDto body)
        {
            int currentUserId = User.GetUserId(); // ✅ ใช้ ID จาก Token

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = await _context.EventCategories.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Category" });

                entity.EventTitle = body.EventTitle.Trim();
                entity.UpdateBy = currentUserId;
                entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();
                return Ok(new { Message = "แก้ไข Category สำเร็จ" });
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
                var entity = await _context.EventCategories.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Category" });

                _context.EventCategories.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Message = "ลบข้อมูลไม่สำเร็จ (Category นี้ถูกใช้งานอยู่ในระบบ)",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        // ✅ GET: /api/event-categories/search?q=chr
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] EventCategorySearchQueryDto req)
        {
            var query = _context.EventCategories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var kw = req.Q.Trim();
                // ✅ ใช้ Contains ธรรมดา เพื่อความปลอดภัย
                query = query.Where(x => x.EventTitle != null && x.EventTitle.Contains(kw));
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
            try
            {
                var data = await _context.EventCategories
                    .AsNoTracking()
                    .OrderBy(x => x.EventTitle)
                    .Select(x => new EventCategoryDropdownDto
                    {
                        Value = x.Id,
                        Label = x.EventTitle ?? "ไม่ระบุหัวข้อ"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Message = "ดึงข้อมูล Category สำหรับ Dropdown สำเร็จ",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "เกิดข้อผิดพลาดในการโหลดข้อมูล",
                    Error = ex.Message
                });
            }
        }
    }
}