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

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _context.JobTitles
            .AsNoTracking()
            .Include(j => j.Department)
            .Select(j => new
            {
                j.Id,
                j.JobsTitle,
                j.DepartmentId,
                DepartmentTitle = j.Department != null ? j.Department.DepartmentTitle : ""
            })
            .ToListAsync());

    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
        => Ok(await _context.JobTitles
            .AsNoTracking()
            .Where(j => j.DepartmentId == departmentId)
            .Select(j => new { j.Id, j.JobsTitle })
            .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobTitleDto dto)
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobTitleDto dto)
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

    // ✅ SEARCH
    // GET: /api/job-titles/search?q=...&departmentId=...
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] JobTitleSearchQueryDto req)
    {
        try
        {
            var query = _context.JobTitles
                .AsNoTracking()
                .Include(j => j.Department)
                .AsQueryable();

            // filter department
            if (req.DepartmentId.HasValue)
                query = query.Where(j => j.DepartmentId == req.DepartmentId.Value);

            // keyword
            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var kw = req.Q.Trim();
                query = query.Where(j =>
                    EF.Functions.Collate((j.JobsTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                    (j.Department != null &&
                     EF.Functions.Collate((j.Department.DepartmentTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw))
                );
            }

            var data = await query
                .OrderByDescending(j => j.Id)
                .Select(j => new
                {
                    j.Id,
                    j.JobsTitle,
                    j.DepartmentId,
                    DepartmentTitle = j.Department != null ? j.Department.DepartmentTitle : ""
                })
                .ToListAsync();

            return Ok(new { Message = "ค้นหาสำเร็จ", Data = data });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "ค้นหาไม่สำเร็จ", Error = ex.Message });
        }
    }
}
