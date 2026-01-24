using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/department-types")]
    public class DepartmentTypesController : ControllerBase
    {
        private readonly AimachineContext _context;
        public DepartmentTypesController(AimachineContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
          => Ok(await _context.DepartmentTypes
            .AsNoTracking()
            .OrderBy(x => x.CanDelete) // false (System Default) ขึ้นก่อน
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.DepartmentTitle,
                x.CreatedAt,
                x.UpdateAt,
                x.CanDelete
            })
            .ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.DepartmentTypes.AsNoTracking()
              .Where(x => x.Id == id)
              .Select(x => new
              {
                  x.Id,
                  x.DepartmentTitle,
                  x.CreatedAt,
                  x.UpdateAt,
                  x.CanDelete
              })
              .FirstOrDefaultAsync();

            return item == null
              ? NotFound(new { Message = "ไม่พบ Department Type" })
              : Ok(item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentTypeDto dto)
        {
            int currentUserId = User.GetUserId();

            if (string.IsNullOrWhiteSpace(dto.DepartmentTitle))
                return BadRequest(new { Message = "กรุณากรอก department_title" });

            // เช็คชื่อซ้ำ (Optional)
            if (await _context.DepartmentTypes.AnyAsync(d => d.DepartmentTitle == dto.DepartmentTitle.Trim()))
                return BadRequest(new { Message = "มีชื่อแผนกนี้อยู่แล้ว" });

            var entity = new DepartmentType
            {
                DepartmentTitle = dto.DepartmentTitle.Trim(),
                CreatedBy = currentUserId,
                UpdateBy = currentUserId,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdateAt = DateTime.UtcNow.AddHours(7),
                CanDelete = true // ✅ Default: สร้างใหม่ให้ลบได้เสมอ
            };

            _context.DepartmentTypes.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentTypeDto dto)
        {
            int currentUserId = User.GetUserId();

            if (string.IsNullOrWhiteSpace(dto.DepartmentTitle))
                return BadRequest(new { Message = "กรุณากรอก department_title" });

            var entity = await _context.DepartmentTypes.FindAsync(id);
            if (entity == null)
                return NotFound(new { Message = "ไม่พบ Department Type" });

            // เช็คชื่อซ้ำกับรายการอื่น (Optional)
            if (await _context.DepartmentTypes.AnyAsync(d => d.Id != id && d.DepartmentTitle == dto.DepartmentTitle.Trim()))
                return BadRequest(new { Message = "มีชื่อแผนกนี้อยู่แล้ว" });

            entity.DepartmentTitle = dto.DepartmentTitle.Trim();
            entity.UpdateBy = currentUserId;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.DepartmentTypes.FindAsync(id);
            if (entity == null)
                return NotFound(new { Message = "ไม่พบ Department Type" });

            // ✅ Check 1: ห้ามลบข้อมูลระบบ (System Default)
            if (entity.CanDelete.HasValue && !entity.CanDelete.Value)
            {
                return BadRequest(new { Message = "ไม่สามารถลบข้อมูลนี้ได้ (System Default)" });
            }

            // ✅ Check 2: ห้ามลบถ้าถูกใช้ใน JobTitle
            if (await _context.JobTitles.AnyAsync(j => j.DepartmentId == id))
            {
                return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจากถูกใช้งานใน Job Title" });
            }

            // ✅ Check 3: ห้ามลบถ้าถูกใช้ใน Partners (เพิ่มส่วนนี้เข้าไปด้วยครับ)
            if (await _context.Partners.AnyAsync(p => p.DepartmentId == id))
            {
                return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจากถูกใช้งานใน Partners" });
            }

            // ✅ Check 4: ห้ามลบถ้าถูกใช้ใน Solutions
            if (await _context.Solutions.AnyAsync(s => s.DepartmentId == id))
            {
                return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจากถูกใช้งานใน Solutions" });
            }

            _context.DepartmentTypes.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] DepartmentSearchQueryDto req)
        {
            try
            {
                var query = _context.DepartmentTypes
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();
                    // ใช้ Contains แทน Collate เพื่อความปลอดภัย
                    query = query.Where(d =>
                        d.DepartmentTitle != null && d.DepartmentTitle.Contains(kw)
                    );
                }

                var data = await query
                    .OrderByDescending(d => d.Id)
                    .Select(d => new
                    {
                        d.Id,
                        d.DepartmentTitle,
                        d.CreatedAt,
                        d.UpdateAt
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
        public async Task<IActionResult> GetDropdown()
        {
            try
            {
                var data = await _context.DepartmentTypes
                    .AsNoTracking()
                    .OrderBy(x => x.DepartmentTitle)
                    .Select(x => new DepartmentSelectionDto
                    {
                        Value = x.Id,
                        Label = x.DepartmentTitle ?? ""
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