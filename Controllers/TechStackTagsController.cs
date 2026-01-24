using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

namespace Aimachine.Controllers
{
	[Route("api/tech-stack-tags")]
	[ApiController]
	public class TechStackTagsController : ControllerBase
	{
		private readonly AimachineContext _context;

		public TechStackTagsController(AimachineContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			// เช็คชื่อ DbSet ตรงนี้ดีๆ นะครับ (TechStackTags หรือ TechStackTag)
			var data = await _context.TechStackTags
				.AsNoTracking()
				.Include(t => t.Department) // Join ตาราง Department
				.OrderByDescending(t => t.Id)
				.Select(t => new
				{
					t.Id,
					t.TechStackTitle, 
					t.DepartmentId,
					DepartmentName = t.Department != null ? t.Department.DepartmentTitle : "",
					t.CreatedAt,
					t.UpdateAt
				})
				.ToListAsync();

			return Ok(data);
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetById(int id)
		{
			var item = await _context.TechStackTags
				.AsNoTracking()
				.Include(t => t.Department)
				.Where(x => x.Id == id)
				.Select(t => new
				{
					t.Id,
					t.TechStackTitle,
					t.DepartmentId,
					DepartmentName = t.Department != null ? t.Department.DepartmentTitle : "",
					t.CreatedAt,
					t.UpdateAt
				})
				.FirstOrDefaultAsync();

			if (item == null) return NotFound(new { Message = "ไม่พบข้อมูล Tech Stack" });

			return Ok(item);
		}


		[HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateTechStackTagDto dto)
		{

            int currentUserId = User.GetUserId();

            if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
				return BadRequest(new { Message = "ไม่พบ Department ID นี้ในระบบ" });

			var entity = new TechStackTag
			{
				DepartmentId = dto.DepartmentId,
				TechStackTitle = dto.TechStackTitle.Trim(), // ✅
				CreatedBy = currentUserId,
				UpdateBy = currentUserId,
				CreatedAt = DateTime.UtcNow.AddHours(7),
				UpdateAt = DateTime.UtcNow.AddHours(7)
			};

			_context.TechStackTags.Add(entity);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "เพิ่ม Tech Stack สำเร็จ", Id = entity.Id });
		}


		[HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTechStackTagDto dto)
		{

            int currentUserId = User.GetUserId();

            var entity = await _context.TechStackTags.FindAsync(id);
			if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Tech Stack" });

			if (entity.DepartmentId != dto.DepartmentId)
			{
				if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
					return BadRequest(new { Message = "ไม่พบ Department ID ที่ระบุ" });
			}

			entity.DepartmentId = dto.DepartmentId;
			entity.TechStackTitle = dto.TechStackTitle.Trim(); // ✅
			entity.UpdateBy = currentUserId;
			entity.UpdateAt = DateTime.UtcNow.AddHours(7);

			await _context.SaveChangesAsync();
			return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
		}

		[HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
		{

			bool isInUse = await _context.TechStackTags1
				.AnyAsync(x => x.TechId == id); 

			if (isInUse)
			{
				return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจาก Tech Stack นี้ถูกนำไปใช้งานแล้ว (มีการผูกข้อมูลอยู่ในระบบ)" });
			}

			var entity = await _context.TechStackTags.FindAsync(id);

			if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Tech Stack" });

			_context.TechStackTags.Remove(entity);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
		}

        [HttpGet("by-department/{departmentId:int}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var data = await _context.TechStackTags
                .AsNoTracking()
                .Where(t => t.DepartmentId == departmentId)
                .OrderBy(t => t.TechStackTitle) // เรียงตามตัวอักษร ก-ฮ, A-Z เพื่อให้หาง่าย
                .Select(t => new
                {
                    t.Id,
                    t.TechStackTitle,
                    t.DepartmentId
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] TechStackTagSearchQueryDto req)
        {
            try
            {
                var query = _context.TechStackTags
                    .AsNoTracking()
                    .Include(t => t.Department)
                    .AsQueryable();

                // 1) filter departmentId (optional)
                if (req.DepartmentId.HasValue)
                    query = query.Where(t => t.DepartmentId == req.DepartmentId.Value);

                // 2) search keyword (optional) - ไม่สนตัวเล็ก/ใหญ่
                if (!string.IsNullOrWhiteSpace(req.Q))
                {
                    var kw = req.Q.Trim();

                    query = query.Where(t =>
                        EF.Functions.Collate((t.TechStackTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw)
                        || (t.Department != null &&
                            EF.Functions.Collate((t.Department.DepartmentTitle ?? ""), "SQL_Latin1_General_CP1_CI_AS").Contains(kw))
                    );
                }

                var data = await query
                    .OrderByDescending(t => t.Id)
                    .Select(t => new
                    {
                        t.Id,
                        t.TechStackTitle,
                        t.DepartmentId,
                        DepartmentName = t.Department != null ? t.Department.DepartmentTitle : "",
                        t.CreatedAt,
                        t.UpdateAt
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