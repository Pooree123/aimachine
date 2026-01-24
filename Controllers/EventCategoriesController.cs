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
            int currentUserId = User.GetUserId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var now = DateTime.UtcNow.AddHours(7);

                // Map ค่าจาก DTO ไปยัง Entity เพื่อเตรียมบันทึกลงฐานข้อมูล
                var entity = new EventCategory
                {
                    EventTitle = body.EventTitle.Trim(),
                    CreatedBy = body.CreatedBy,
                    UpdateBy = currentUserId, // เริ่มต้นให้ค่า UpdateBy เท่ากับคนสร้าง
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventCategoryDto body) // เปลี่ยนจาก EventCategory เป็น DTO
        {
            int currentUserId = User.GetUserId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entity = await _context.EventCategories.FindAsync(id);
                if (entity == null)
                    return NotFound(new { Message = "ไม่พบ Category" });

                // อัปเดตข้อมูลจาก DTO ลงใน Entity
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
            try
            {
                var data = await _context.EventCategories
                    .AsNoTracking()
                    .OrderBy(x => x.EventTitle) // เรียงลำดับตามตัวอักษร
                    .Select(x => new EventCategoryDropdownDto
                    {
                        Value = x.Id,
                        Label = x.EventTitle ?? "ไม่ระบุหัวข้อ" // จัดการกรณีข้อมูลเป็น Null
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
                // ส่งข้อความ Error กลับไปเพื่อให้รู้ว่าเกิดปัญหาที่จุดไหน
                return BadRequest(new
                {
                    Message = "เกิดข้อผิดพลาดในการโหลดข้อมูล",
                    Error = ex.Message
                });
            }
        }

    }
}