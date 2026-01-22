using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers;

[ApiController]
[Route("api/department-types")]
public class DepartmentTypesController : ControllerBase
{
    private readonly AimachineContext _context;
    public DepartmentTypesController(AimachineContext context) => _context = context;

    // GET: All
    [HttpGet]
    public async Task<IActionResult> GetAll()
      => Ok(await _context.DepartmentTypes
        .AsNoTracking()
        .OrderByDescending(x => x.Id)
        .Select(x => new
        {
            x.Id,
            x.DepartmentTitle,
            x.CreatedAt,
            x.UpdateAt,
            x.CanDelete // ✅ ส่งค่า CanDelete ไปด้วย
        })
        .ToListAsync());

    // GET: By Id
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
              x.CanDelete // ✅ ส่งค่า CanDelete ไปด้วย
          })
          .FirstOrDefaultAsync();

        return item == null
          ? NotFound(new { Message = "ไม่พบ Department Type" })
          : Ok(item);
    }

    // POST: Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentTypeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DepartmentTitle))
            return BadRequest(new { Message = "กรุณากรอก department_title" });

        var entity = new DepartmentType
        {
            DepartmentTitle = dto.DepartmentTitle.Trim(),
            CreatedBy = dto.CreatedBy,
            UpdateBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            UpdateAt = DateTime.UtcNow.AddHours(7),
            CanDelete = true // ✅ Default: ให้ลบได้สำหรับข้อมูลที่สร้างใหม่
        };

        _context.DepartmentTypes.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
    }

    // PUT: Update
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentTypeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DepartmentTitle))
            return BadRequest(new { Message = "กรุณากรอก department_title" });

        var entity = await _context.DepartmentTypes.FindAsync(id);
        if (entity == null)
            return NotFound(new { Message = "ไม่พบ Department Type" });

        entity.DepartmentTitle = dto.DepartmentTitle.Trim();
        entity.UpdateBy = dto.UpdateBy;
        entity.UpdateAt = DateTime.UtcNow.AddHours(7);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
    }

    // DELETE: Delete
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.DepartmentTypes.FindAsync(id);
        if (entity == null)
        {
            return NotFound(new { Message = "ไม่พบ Department Type" });
        }

        // ✅ Check 1: ตรวจสอบสิทธิ์การลบจากฐานข้อมูล (CanDelete)
        // ถ้า CanDelete เป็น false หรือ null (แล้วแต่นโยบาย ถ้า null คือห้ามลบก็แก้เงื่อนไขได้)
        if (entity.CanDelete.HasValue && !entity.CanDelete.Value)
        {
            return BadRequest(new { Message = "ไม่สามารถลบข้อมูลนี้ได้ (System Default)" });
        }

        // ✅ Check 2: กันลบ ถ้ายังมี JobTitle ใช้งานอยู่ (Foreign Key Check)
        if (await _context.JobTitles.AnyAsync(j => j.DepartmentId == id))
        {
            return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจากยังมี Job Title ใช้งานอยู่" });
        }

        _context.DepartmentTypes.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
    }
}