using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers;

[ApiController]
[Route("api/job-titles")]
public class JobTitlesController : ControllerBase
{
    private readonly AimachineContext _context;
    public JobTitlesController(AimachineContext context) => _context = context;

    // GET: all (พร้อมชื่อ department)
    [HttpGet]
    public async Task<IActionResult> GetAll()
  => Ok(await _context.JobTitles
    .AsNoTracking()
    .Include(j => j.Department) // ✅ ใช้ Department
        .Select(j => new {
            j.Id,
            j.JobsTitle,
            j.DepartmentId,
            DepartmentTitle = j.Department.DepartmentTitle
        })
    .ToListAsync());


    // GET: by department (one -> many)
    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
  => Ok(await _context.JobTitles
    .AsNoTracking()
    .Where(j => j.DepartmentId == departmentId)
    .Select(j => new { j.Id, j.JobsTitle })
    .ToListAsync());


    // POST
    [HttpPost]
    public async Task<IActionResult> Create(CreateJobTitleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.JobsTitle))
            return BadRequest(new { Message = "กรุณากรอก jobs_title" });

        if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
            return BadRequest(new { Message = "ไม่พบ department_type" });

        var entity = new JobTitle
        {
            DepartmentId = dto.DepartmentId,
            JobsTitle = dto.JobsTitle.Trim(),
            CreatedBy = dto.CreatedBy,
            UpdateBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            UpdateAt = DateTime.UtcNow.AddHours(7)
        };

        _context.JobTitles.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
    }

    // PUT
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateJobTitleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.JobsTitle))
            return BadRequest(new { Message = "กรุณากรอก jobs_title" });

        var entity = await _context.JobTitles.FindAsync(id);
        if (entity == null)
            return NotFound(new { Message = "ไม่พบ Job Title" });

        entity.JobsTitle = dto.JobsTitle.Trim();
        entity.UpdateBy = dto.UpdateBy;
        entity.UpdateAt = DateTime.UtcNow.AddHours(7);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
    }

    // DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.JobTitles.FindAsync(id);
        if (entity == null)
            return NotFound(new { Message = "ไม่พบ Job Title" });

        _context.JobTitles.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
    }
}
