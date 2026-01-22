using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // =============================================
        // ✅ GET: ดึงข้อมูลงานทั้งหมด
        // =============================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Jobs
                .AsNoTracking()
                .Include(j => j.JobTitle)
                .Include(j => j.JobsTags)
                .OrderByDescending(j => j.Id)
                .Select(j => new
                {
                    j.Id,
                    j.Description,
                    j.Status,
                    j.TotalPositions,
                    j.DateOpen,
                    j.DateEnd,

                    // ✅ แก้ไข 1: ดึง JobTitle ให้ตรงกับ Model (JobsTitle)
                    j.JobTitleId,
                    JobTitleName = j.JobTitle != null ? j.JobTitle.JobsTitle : "",

                    // ✅ แก้ไข 2: ดึง Tag ID ให้ตรงกับ Model (StackTagId)
                    // ใช้ ?? 0 เพื่อกันค่า Null เพราะใน DB เป็น int?
                    TechStackTagIds = j.JobsTags.Select(t => t.StackTagId ?? 0).ToList(),

                    j.CreatedAt,
                    j.UpdateAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // =============================================
        // ✅ GET: ดึงตาม ID
        // =============================================
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
                    // ✅ แก้ไขชื่อตัวแปร
                    JobTitleName = j.JobTitle != null ? j.JobTitle.JobsTitle : "",

                    // ✅ แก้ไขชื่อตัวแปร FK
                    TechStackTagIds = j.JobsTags.Select(t => t.StackTagId ?? 0).ToList(),

                    j.CreatedAt,
                    j.UpdateAt
                })
                .FirstOrDefaultAsync();

            if (job == null) return NotFound(new { Message = "ไม่พบประกาศงาน" });

            return Ok(job);
        }

        // =============================================
        // ✅ POST: เพิ่มประกาศงานใหม่
        // =============================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJobDto dto)
        {
            if (!await _context.JobTitles.AnyAsync(jt => jt.Id == dto.JobTitleId))
                return BadRequest(new { Message = "ไม่พบ JobTitle ID นี้" });

            var entity = new Job
            {
                JobTitleId = dto.JobTitleId,
                Description = dto.Description,
                TotalPositions = dto.TotalPositions,
                DateOpen = dto.DateOpen,
                DateEnd = dto.DateEnd,
                Status = dto.Status,
                CreatedBy = dto.CreatedBy,
                UpdateBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdateAt = DateTime.UtcNow.AddHours(7)
            };

            _context.Jobs.Add(entity);
            await _context.SaveChangesAsync();

            // ✅ บันทึก Tags
            if (dto.TechStackTagIds != null && dto.TechStackTagIds.Count > 0)
            {
                foreach (var tagId in dto.TechStackTagIds)
                {
                    var jobTag = new JobsTag
                    {
                        JobId = entity.Id,

                        // ✅ แก้ไข 3: ใช้ชื่อ StackTagId ให้ตรงกับ Model
                        StackTagId = tagId
                    };
                    _context.JobsTags.Add(jobTag);
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "สร้างประกาศงานสำเร็จ", Id = entity.Id });
        }

        // =============================================
        // ✅ PUT: แก้ไขข้อมูล
        // =============================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateJobDto dto)
        {
            var entity = await _context.Jobs.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบประกาศงาน" });

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
            entity.Status = dto.Status;
            entity.UpdateBy = dto.UpdateBy;
            entity.UpdateAt = DateTime.UtcNow.AddHours(7);

            // อัปเดต Tags
            if (dto.TechStackTagIds != null)
            {
                var oldTags = _context.JobsTags.Where(x => x.JobId == id);
                _context.JobsTags.RemoveRange(oldTags);

                foreach (var tagId in dto.TechStackTagIds)
                {
                    var jobTag = new JobsTag
                    {
                        JobId = entity.Id,
                        // ✅ แก้ไข 4: ใช้ชื่อ StackTagId
                        StackTagId = tagId
                    };
                    _context.JobsTags.Add(jobTag);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

        // =============================================
        // ✅ DELETE
        // =============================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Jobs.FindAsync(id);
            if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล" });

            var relatedTags = _context.JobsTags.Where(t => t.JobId == id);
            _context.JobsTags.RemoveRange(relatedTags);

            _context.Jobs.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
        }
    }
}