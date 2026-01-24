using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternsController : ControllerBase
    {
        private readonly AimachineContext _context;

        public InternsController(AimachineContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // ไม่จำเป็นต้องใช้ตัวแปร today ใน Query แล้ว แต่ประกาศไว้ใช้ที่อื่นได้
            var today = DateTime.UtcNow.AddHours(7);

            var data = await _context.Interns
                .AsNoTracking()
                .Include(i => i.JobTitle)
                    .ThenInclude(j => j.Department)
                .Include(i => i.InternTags)
                    .ThenInclude(it => it.StackTag)

                // ✅ แก้ไขตรงนี้: เปลี่ยน Logic ให้ EF Core แปลภาษา SQL ได้ง่ายๆ
                // 1. OrderByDescending(HasValue) -> เอาคนที่มีวันปิดรับ (True) ขึ้นก่อน, ไม่มี (False) ไว้ล่าง
                // 2. ThenBy(DateEnd) -> เรียงวันปิดรับจาก "น้อยไปมาก" (อดีต -> อนาคต) 
                //    ผลลัพธ์จะเหมือนกับสูตร (Today - DateEnd) เรียงจาก "มากไปน้อย"
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
                    JobTitleName = i.JobTitle.JobsTitle,
                    i.Description,
                    i.TotalPositions,
                    i.DateOpen,
                    i.DateEnd,
                    i.Status,
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

        // ✅ แก้ไข: POST (ใช้ Strategy Wrap Transaction)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateInternDto dto)
        {

            int currentUserId = User.GetUserId();

            if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
                return BadRequest(new { Message = "ไม่พบ JobTitle ID ที่ระบุ" });

            // สร้าง Strategy สำหรับจัดการ Retry Policy
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. สร้าง Intern
                    var entity = new Intern
                    {
                        JobTitleId = dto.JobTitleId,
                        Description = dto.Description,
                        TotalPositions = dto.TotalPositions,
                        DateOpen = dto.DateOpen,
                        DateEnd = dto.DateEnd,
                        Status = dto.Status,
                        CreatedBy = currentUserId,
                        UpdateBy = currentUserId,
                        CreatedAt = DateTime.UtcNow.AddHours(7),
                        UpdateAt = DateTime.UtcNow.AddHours(7)
                    };

                    _context.Interns.Add(entity);
                    await _context.SaveChangesAsync();

                    // 2. บันทึก Tags
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

        // ✅ แก้ไข: PUT (ใช้ Strategy Wrap Transaction)
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInternDto dto)
        {
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

                    if (!await _context.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
                        return BadRequest(new { Message = "ไม่พบ JobTitle ID ที่ระบุ" });

                    // 1. อัปเดตข้อมูล
                    entity.JobTitleId = dto.JobTitleId;
                    entity.Description = dto.Description;
                    entity.TotalPositions = dto.TotalPositions;
                    entity.DateOpen = dto.DateOpen;
                    entity.DateEnd = dto.DateEnd;
                    entity.Status = dto.Status;
                    entity.UpdateBy = currentUserId;
                    entity.UpdateAt = DateTime.UtcNow.AddHours(7);

                    // 2. จัดการ Tags (ลบเก่า -> ใส่ใหม่)
                    if (entity.InternTags.Any())
                    {
                        _context.InternTags.RemoveRange(entity.InternTags);
                    }

                    if (dto.StackTagIds != null && dto.StackTagIds.Count > 0)
                    {
                        var newTags = dto.StackTagIds.Select(tagId => new InternTag
                        {
                            InternId = entity.Id,
                            StackTagId = tagId
                        }).ToList();
                        _context.InternTags.AddRange(newTags);
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

        // DELETE (ไม่ต้องใช้ Transaction เพราะลบทีเดียวจบ)
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Interns
                .Include(i => i.InternTags)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Intern" });

            try
            {
                if (entity.InternTags.Any())
                {
                    _context.InternTags.RemoveRange(entity.InternTags);
                }

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

                // 1) filter department (optional) จาก dropdown
                if (req.DepartmentId.HasValue)
                {
                    query = query.Where(i =>
                        i.JobTitle != null &&
                        i.JobTitle.DepartmentId == req.DepartmentId.Value
                    );
                }

                // 2) search keyword (optional) - ไม่สนตัวเล็ก/ใหญ่
                // ค้นได้จาก: ชื่องาน(JobTitle), รายละเอียด(Description), ชื่อ Tag(TechStackTitle)
                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();

                    query = query.Where(i =>
                        EF.Functions.Collate((i.Description ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                        || (i.JobTitle != null &&
                            EF.Functions.Collate((i.JobTitle.JobsTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw))
                        || i.InternTags.Any(t =>
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
                        Tags = i.InternTags.Select(t => new
                        {
                            t.StackTagId,
                            TagName = t.StackTag != null ? t.StackTag.TechStackTitle : ""
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