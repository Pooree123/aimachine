using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
    [Route("api/interns")]
    [ApiController]
    public class InternsController : ControllerBase
    {
        private readonly AimachineContext _context;

        public InternsController(AimachineContext context)
        {
            _context = context;
        }

        // ✅ 1. Public: ดึงเฉพาะ Active (สำหรับหน้าบ้าน)
        [HttpGet]
        public async Task<IActionResult> GetPublicInterns()
        {
            var data = await _context.Interns
                .AsNoTracking()
                .Include(i => i.JobTitle)
                    .ThenInclude(j => j.Department)
                .Include(i => i.InternTags)
                    .ThenInclude(it => it.StackTag)
                .Where(i => i.Status == "Active") // 👈 กรองเฉพาะ Active
                                                  // เรียงลำดับ: มีวันปิดรับขึ้นก่อน, วันปิดรับน้อยไปมาก (รีบปิดก่อนขึ้นก่อน)
                .OrderByDescending(i => i.DateEnd.HasValue)
                .ThenBy(i => i.DateEnd)
                .Select(i => new
                {
                    i.Id,
                    i.JobTitleId,
                    JobTitleName = i.JobTitle != null ? i.JobTitle.JobsTitle : "",
                    DepartmentId = i.JobTitle != null ? i.JobTitle.DepartmentId : (int?)null,
                    DepartmentName = (i.JobTitle != null && i.JobTitle.Department != null)
                                     ? i.JobTitle.Department.DepartmentTitle
                                     : "",
                    i.Description,
                    i.TotalPositions,
                    i.DateOpen,
                    i.DateEnd,
                    i.Status,
                    TechStacks = i.InternTags.Select(it => new
                    {
                        Id = it.StackTagId,
                        Name = it.StackTag != null ? it.StackTag.TechStackTitle : ""
                    }).ToList(),
                    i.CreatedAt,
                    i.UpdateAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ 2. Admin: ดึงทั้งหมด (Active + InActive)
        [HttpGet("admin")]
        [Authorize]
        public async Task<IActionResult> GetAdminInterns()
        {
            var data = await _context.Interns
                .AsNoTracking()
                .Include(i => i.JobTitle)
                    .ThenInclude(j => j.Department)
                .Include(i => i.InternTags)
                    .ThenInclude(it => it.StackTag)
                // 👈 ไม่ต้องกรอง Status เพื่อให้ Admin เห็นทั้งหมด
                .OrderByDescending(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    i.JobTitleId,
                    JobTitleName = i.JobTitle != null ? i.JobTitle.JobsTitle : "",
                    DepartmentId = i.JobTitle != null ? i.JobTitle.DepartmentId : (int?)null,
                    DepartmentName = (i.JobTitle != null && i.JobTitle.Department != null)
                                     ? i.JobTitle.Department.DepartmentTitle
                                     : "",
                    i.Description,
                    i.TotalPositions,
                    i.DateOpen,
                    i.DateEnd,
                    i.Status,
                    TechStacks = i.InternTags.Select(it => new
                    {
                        Id = it.StackTagId,
                        Name = it.StackTag != null ? it.StackTag.TechStackTitle : ""
                    }).ToList(),
                    i.CreatedAt,
                    i.UpdateAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: GetById
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Interns
                .AsNoTracking()
                .Include(i => i.JobTitle)
                .Include(i => i.InternTags)
                    .ThenInclude(it => it.StackTag)
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    i.JobTitleId,
                    JobTitleName = i.JobTitle != null ? i.JobTitle.JobsTitle : "",
                    DepartmentId = i.JobTitle != null ? i.JobTitle.DepartmentId : (int?)null,
                    i.Description,
                    i.TotalPositions,
                    i.DateOpen,
                    i.DateEnd,
                    i.Status,
                    // ส่ง ID Array ไปให้หน้าบ้าน Mapping (สำหรับ Auto-fill ใน Form)
                    StackTagIds = i.InternTags.Select(t => t.StackTagId).ToList(),
                    Tags = i.InternTags.Select(t => new
                    {
                        t.StackTagId,
                        TagName = t.StackTag != null ? t.StackTag.TechStackTitle : ""
                    }).ToList(),
                    i.CreatedAt,
                    i.UpdateAt
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound(new { Message = "ไม่พบข้อมูล Intern" });

            return Ok(item);
        }

        // ✅ 3. Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateInternDto dto)
        {
            if (dto == null) return BadRequest(new { Message = "ไม่พบข้อมูลที่ส่งมา" });
            if (!ModelState.IsValid) return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            int currentUserId = User.GetUserId();

            if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
                return BadRequest(new { Message = "ไม่พบ JobTitle ID ที่ระบุ" });

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var entity = new Intern
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

                    _context.Interns.Add(entity);
                    await _context.SaveChangesAsync();

                    if (dto.StackTagIds != null && dto.StackTagIds.Count > 0)
                    {
                        var tags = dto.StackTagIds.Select(tagId => new InternTag
                        {
                            InternId = entity.Id,
                            StackTagId = tagId
                        }).ToList();

                        _context.InternTags.AddRange(tags);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return Ok(new { Message = "เพิ่มข้อมูลสำเร็จ", Id = entity.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "เพิ่มข้อมูลไม่สำเร็จ", Error = ex.Message });
                }
            });
        }

        // ✅ 4. Update
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInternDto dto)
        {
            if (dto == null) return BadRequest(new { Message = "ไม่พบข้อมูลที่ส่งมา" });
            if (!ModelState.IsValid) return BadRequest(new { Message = "ข้อมูลไม่ถูกต้อง", Errors = ModelState });

            int currentUserId = User.GetUserId();
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var entity = await _context.Interns
                        .Include(i => i.InternTags)
                        .FirstOrDefaultAsync(i => i.Id == id);

                    if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Intern" });

                    if (entity.JobTitleId != dto.JobTitleId)
                    {
                        if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
                            return BadRequest(new { Message = "ไม่พบ JobTitle ID ที่ระบุ" });
                    }

                    // อัปเดตข้อมูล
                    entity.JobTitleId = dto.JobTitleId;
                    entity.Description = dto.Description;
                    entity.TotalPositions = dto.TotalPositions;
                    entity.DateOpen = dto.DateOpen;
                    entity.DateEnd = dto.DateEnd;
                    entity.Status = dto.Status;
                    entity.UpdateBy = currentUserId;
                    entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                    // อัปเดต Tags (ลบเก่า -> ใส่ใหม่)
                    if (dto.StackTagIds != null)
                    {
                        // ลบ Tags เก่าออกให้หมดก่อน
                        if (entity.InternTags.Any())
                        {
                            _context.InternTags.RemoveRange(entity.InternTags);
                        }

                        // เพิ่ม Tags ใหม่เข้าไป
                        if (dto.StackTagIds.Count > 0)
                        {
                            var newTags = dto.StackTagIds.Select(tagId => new InternTag
                            {
                                InternId = entity.Id,
                                StackTagId = tagId
                            }).ToList();
                            _context.InternTags.AddRange(newTags);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
                }
            });
        }

        // ✅ 5. Delete: ลบจริง (Hard Delete) - ตัดเรื่อง Inbox ออกแล้ว
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _context.Interns
                    .Include(i => i.InternTags)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Intern" });

                // ลบ Tags ที่เกี่ยวข้องก่อน
                if (entity.InternTags.Any())
                {
                    _context.InternTags.RemoveRange(entity.InternTags);
                }

                // ลบ Intern จริง
                _context.Interns.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ลบข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] InternSearchQueryDto req)
        {
            try
            {
                var query = _context.Interns
                    .AsNoTracking()
                    .Include(i => i.JobTitle)
                        .ThenInclude(j => j.Department)
                    .Include(i => i.InternTags)
                        .ThenInclude(it => it.StackTag)
                    .AsQueryable();

                // 1) filter department
                if (req.DepartmentId.HasValue)
                {
                    query = query.Where(i => i.JobTitle != null && i.JobTitle.DepartmentId == req.DepartmentId.Value);
                }

                // 2) search keyword
                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();
                    query = query.Where(i =>
                        EF.Functions.Collate((i.Description ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw) ||
                        (i.JobTitle != null &&
                         EF.Functions.Collate((i.JobTitle.JobsTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)) ||
                        i.InternTags.Any(t =>
                            t.StackTag != null &&
                            EF.Functions.Collate((t.StackTag.TechStackTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                        )
                    );
                }

                var data = await query
                    .OrderByDescending(i => i.Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.JobTitleId,
                        JobTitleName = i.JobTitle != null ? i.JobTitle.JobsTitle : "",
                        DepartmentId = i.JobTitle != null ? i.JobTitle.DepartmentId : (int?)null,
                        DepartmentName = (i.JobTitle != null && i.JobTitle.Department != null) ? i.JobTitle.Department.DepartmentTitle : "",
                        i.Description,
                        i.TotalPositions,
                        i.DateOpen,
                        i.DateEnd,
                        i.Status,
                        TechStacks = i.InternTags.Select(t => new
                        {
                            Id = t.StackTagId,
                            Name = t.StackTag != null ? t.StackTag.TechStackTitle : ""
                        }).ToList(),
                        i.CreatedAt,
                        i.UpdateAt
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
}