using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

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
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateJobTitleDto dto)
    {
        int currentUserId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(dto.JobsTitle))
            return BadRequest(new { Message = "กรุณากรอก jobs_title" });

        if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
            return BadRequest(new { Message = "ไม่พบ department_type" });

        var entity = new JobTitle
        {
            DepartmentId = dto.DepartmentId,
            JobsTitle = dto.JobsTitle.Trim(),
            CreatedBy = currentUserId,
            UpdateBy = currentUserId,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            UpdateAt = DateTime.UtcNow.AddHours(7)
        };

        _context.JobTitles.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobTitleDto dto)
    {
        int currentUserId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(dto.JobsTitle))
            return BadRequest(new { Message = "กรุณากรอก jobs_title" });

        var entity = await _context.JobTitles.FindAsync(id);
        if (entity == null)
            return NotFound(new { Message = "ไม่พบ Job Title" });

        // เช็คและอัปเดต DepartmentId
        if (entity.DepartmentId != dto.DepartmentId)
        {
            bool deptExists = await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists)
            {
                return BadRequest(new { Message = "ไม่พบ department_type ที่ระบุ" });
            }
            entity.DepartmentId = dto.DepartmentId;
        }

        entity.JobsTitle = dto.JobsTitle.Trim();
        entity.UpdateBy = currentUserId;
        entity.UpdateAt = DateTime.UtcNow.AddHours(7);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
    }

    // ✅ ปรับปรุง Delete: เช็คการใช้งานก่อนลบ
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.JobTitles.FindAsync(id);
        if (entity == null)
            return NotFound(new { Message = "ไม่พบ Job Title" });

        // 1. เช็คว่าถูกใช้ใน Jobs (ประกาศงาน) หรือไม่
        bool isUsedInJobs = await _context.Jobs.AnyAsync(j => j.JobTitleId == id);
        if (isUsedInJobs)
        {
            return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจาก Job Title นี้ถูกใช้งานอยู่ในประกาศรับสมัครงาน (Jobs)" });
        }

        // 2. เช็คว่าถูกใช้ใน Interns (ฝึกงาน) หรือไม่
        bool isUsedInInterns = await _context.Interns.AnyAsync(i => i.JobTitleId == id);
        if (isUsedInInterns)
        {
            return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจาก Job Title นี้ถูกใช้งานอยู่ในประกาศฝึกงาน (Interns)" });
        }

        // 3. เช็คว่าถูกใช้ใน Comments (รีวิว/Testimonials) หรือไม่
        bool isUsedInComments = await _context.Comments.AnyAsync(c => c.JobTitleId == id);
        if (isUsedInComments)
        {
            return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจาก Job Title นี้ถูกใช้งานอยู่ในรีวิว (Comments)" });
        }

        _context.JobTitles.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] JobTitleSearchQueryDto req)
    {
        try
        {
            var query = _context.JobTitles
                .AsNoTracking()
                .Include(j => j.Department)
                .AsQueryable();

            if (req.DepartmentId.HasValue)
                query = query.Where(j => j.DepartmentId == req.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var kw = req.Q.Trim();
                // ใช้ Contains แทน Collate เพื่อความปลอดภัย
                query = query.Where(j =>
                    (j.JobsTitle != null && j.JobsTitle.Contains(kw)) ||
                    (j.Department != null && j.Department.DepartmentTitle != null && j.Department.DepartmentTitle.Contains(kw))
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