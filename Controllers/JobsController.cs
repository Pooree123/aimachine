using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly AimachineContext _context;

        public JobsController(AimachineContext context)
        {
            _context = context;
        }

        // ✅ 1. Public: ดึงเฉพาะ Active (สำหรับหน้าบ้าน)
        [HttpGet]
        public async Task<IActionResult> GetPublicJobs()
        {
            var data = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.Status == "Active") // 👈 กรองเฉพาะ Active
                .OrderByDescending(j => j.Id)
                .Select(j => new
                {
                    j.Id,
                    j.Description,
                    j.Status,
                    j.TotalPositions,
                    j.DateOpen,
                    j.DateEnd,
                    j.JobTitleId,
                    JobTitleName = j.JobTitle != null ? j.JobTitle.JobsTitle : "",
                    DepartmentId = j.JobTitle != null ? j.JobTitle.DepartmentId : (int?)null,
                    DepartmentName = (j.JobTitle != null && j.JobTitle.Department != null) ? j.JobTitle.Department.DepartmentTitle : "",
                    TechStacks = j.JobsTags.Select(jt => new
                    {
                        Id = jt.StackTagId,
                        Name = jt.StackTag != null ? jt.StackTag.TechStackTitle : ""
                    }).ToList(),
                    j.CreatedAt,
                    j.UpdateAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 2. Admin: ดึงทั้งหมด (Active + InActive)
        [HttpGet("admin")]
        [Authorize]
        public async Task<IActionResult> GetAdminJobs()
        {
            var data = await _context.Jobs
                .AsNoTracking()
                .OrderByDescending(j => j.Id)
                // 👈 ไม่ต้องกรอง Status เพื่อให้ Admin เห็นทั้งหมด
                .Select(j => new
                {
                    j.Id,
                    j.Description,
                    j.Status,
                    j.TotalPositions,
                    j.DateOpen,
                    j.DateEnd,
                    j.JobTitleId,
                    JobTitleName = j.JobTitle != null ? j.JobTitle.JobsTitle : "",
                    DepartmentId = j.JobTitle != null ? j.JobTitle.DepartmentId : (int?)null,
                    DepartmentName = (j.JobTitle != null && j.JobTitle.Department != null) ? j.JobTitle.Department.DepartmentTitle : "",
                    TechStacks = j.JobsTags.Select(jt => new
                    {
                        Id = jt.StackTagId,
                        Name = jt.StackTag != null ? jt.StackTag.TechStackTitle : ""
                    }).ToList(),
                    j.CreatedAt,
                    j.UpdateAt
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .Include(j => j.JobTitle)
                .Include(j => j.JobsTags)
                .Where(x => x.Id == id)
                .Select(j => new
                {
                    j.Id,
                    j.Description,
                    j.Status,
                    j.TotalPositions,
                    j.DateOpen,
                    j.DateEnd,
                    j.JobTitleId,
                    JobTitleName = j.JobTitle != null ? j.JobTitle.JobsTitle : "",
                    DepartmentId = j.JobTitle != null ? j.JobTitle.DepartmentId : (int?)null,
                    TechStackTagIds = j.JobsTags.Select(t => t.StackTagId ?? 0).ToList(), // ส่งเป็น ID array ไปให้หน้าบ้านแมพ
                    j.CreatedAt,
                    j.UpdateAt
                })
                .FirstOrDefaultAsync();

            if (job == null) return NotFound(new { Message = "ไม่พบประกาศงาน" });

            return Ok(job);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateJobDto dto)
        {
            int currentUserId = User.GetUserId();

            if (!await _context.JobTitles.AnyAsync(jt => jt.Id == dto.JobTitleId))
                return BadRequest(new { Message = "ไม่พบ JobTitle ID นี้" });

            var entity = new Job
            {
                JobTitleId = dto.JobTitleId,
                Description = dto.Description,
                TotalPositions = dto.TotalPositions,
                DateOpen = dto.DateOpen,
                DateEnd = dto.DateEnd,
                Status = dto.Status ?? "Active", // Default Active
                CreatedBy = currentUserId,
                UpdateBy = currentUserId,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdateAt = DateTime.UtcNow.AddHours(7)
            };

            _context.Jobs.Add(entity);
            await _context.SaveChangesAsync();

            // เพิ่ม Tech Stacks
            if (dto.TechStackTagIds != null && dto.TechStackTagIds.Count > 0)
            {
                foreach (var tagId in dto.TechStackTagIds)
                {
                    var jobTag = new JobsTag
                    {
                        JobId = entity.Id,
                        StackTagId = tagId
                    };
                    _context.JobsTags.Add(jobTag);
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "สร้างประกาศงานสำเร็จ", Id = entity.Id });
        }

        // ✅ 3. Update: แก้ไขได้ทุกอย่างรวมถึง Status
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateJobDto dto)
        {
            int currentUserId = User.GetUserId();

            var entity = await _context.Jobs.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบประกาศงาน" });

            // ตรวจสอบ JobTitle ใหม่ว่ามีจริงไหม
            if (entity.JobTitleId != dto.JobTitleId)
            {
                if (!await _context.JobTitles.AnyAsync(jt => jt.Id == dto.JobTitleId))
                    return BadRequest(new { Message = "ไม่พบ JobTitle ID ใหม่ที่ระบุ" });
            }

            // อัปเดตข้อมูล
            entity.JobTitleId = dto.JobTitleId;
            entity.Description = dto.Description;
            entity.TotalPositions = dto.TotalPositions;
            entity.DateOpen = dto.DateOpen;
            entity.DateEnd = dto.DateEnd;
            entity.Status = dto.Status; // 👈 อัปเดต Status (Active/Inactive)
            entity.UpdateBy = currentUserId;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);

            // อัปเดต Tech Stacks (ลบของเก่า -> ใส่ของใหม่)
            if (dto.TechStackTagIds != null)
            {
                var oldTags = _context.JobsTags.Where(x => x.JobId == id);
                _context.JobsTags.RemoveRange(oldTags);

                foreach (var tagId in dto.TechStackTagIds)
                {
                    var jobTag = new JobsTag
                    {
                        JobId = entity.Id,
                        StackTagId = tagId
                    };
                    _context.JobsTags.Add(jobTag);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

        // ✅ 4. Delete: ลบจริง (Hard Delete)
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Jobs.FindAsync(id);
                if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล" });

                // 1. ลบ Tags ที่เกี่ยวข้องก่อน (เพราะเป็น Foreign Key)
                var relatedTags = _context.JobsTags.Where(t => t.JobId == id);
                _context.JobsTags.RemoveRange(relatedTags);

                // 2. ลบ Job จริง
                _context.Jobs.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (DbUpdateException ex)
            {
                // กันกรณีลบไม่ได้เพราะมีคนสมัครงานแล้ว (ติด FK ที่ตาราง Inbox)
                return BadRequest(new
                {
                    Message = "ไม่สามารถลบได้ เนื่องจากมีการสมัครงานเข้ามาในตำแหน่งนี้แล้ว",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] JobSearchQueryDto req)
        {
            var query = _context.Jobs
                .AsNoTracking()
                .Include(j => j.JobTitle)
                .Include(j => j.JobsTags)
                .AsQueryable();

            // Search Filter
            if (req.JobTitleId.HasValue)
                query = query.Where(j => j.JobTitleId == req.JobTitleId.Value);

            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var kw = req.Q.Trim();
                query = query.Where(j =>
                    EF.Functions.Collate((j.Description ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                    (j.JobTitle != null &&
                     EF.Functions.Collate((j.JobTitle.JobsTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw))
                );
            }

            var data = await query
                .OrderByDescending(j => j.Id)
                .Select(j => new
                {
                    j.Id,
                    j.Description,
                    j.Status,
                    j.TotalPositions,
                    j.DateOpen,
                    j.DateEnd,
                    j.JobTitleId,
                    JobTitleName = j.JobTitle != null ? j.JobTitle.JobsTitle : "",
                    DepartmentId = j.JobTitle != null ? j.JobTitle.DepartmentId : (int?)null,
                    TechStackTagIds = j.JobsTags.Select(t => t.StackTagId ?? 0).ToList(),
                    j.CreatedAt,
                    j.UpdateAt
                })
                .ToListAsync();

            return Ok(new { Message = "ค้นหาสำเร็จ", Data = data });
        }
    }
}