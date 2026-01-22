using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

		// =============================================
		// ✅ GET: ดึงข้อมูลทั้งหมด + ชื่อแผนก
		// =============================================
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
					t.TechStackTitle, // ✅ ชื่อใหม่
					t.DepartmentId,
					DepartmentName = t.Department != null ? t.Department.DepartmentTitle : "",
					t.CreatedAt,
					t.UpdateAt
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

		// =============================================
		// ✅ POST: เพิ่มข้อมูลใหม่
		// =============================================
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateTechStackTagDto dto)
		{
			// 1. ตรวจสอบว่ามี Department จริงไหม
			if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
				return BadRequest(new { Message = "ไม่พบ Department ID นี้ในระบบ" });

			// 2. สร้าง Entity (ใช้ชื่อใหม่ TechStackTitle)
			var entity = new TechStackTag
			{
				DepartmentId = dto.DepartmentId,
				TechStackTitle = dto.TechStackTitle.Trim(), // ✅
				CreatedBy = dto.CreatedBy,
				UpdateBy = dto.CreatedBy,
				CreatedAt = DateTime.UtcNow.AddHours(7),
				UpdateAt = DateTime.UtcNow.AddHours(7)
			};

			_context.TechStackTags.Add(entity);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "เพิ่ม Tech Stack สำเร็จ", Id = entity.Id });
		}

		// =============================================
		// ✅ PUT: แก้ไขข้อมูล
		// =============================================
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateTechStackTagDto dto)
		{
			var entity = await _context.TechStackTags.FindAsync(id);
			if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Tech Stack" });

			// เช็ค Department ถ้ามีการเปลี่ยน
			if (entity.DepartmentId != dto.DepartmentId)
			{
				if (!await _context.DepartmentTypes.AnyAsync(d => d.Id == dto.DepartmentId))
					return BadRequest(new { Message = "ไม่พบ Department ID ที่ระบุ" });
			}

			// อัปเดตข้อมูล
			entity.DepartmentId = dto.DepartmentId;
			entity.TechStackTitle = dto.TechStackTitle.Trim(); // ✅
			entity.UpdateBy = dto.UpdateBy;
			entity.UpdateAt = DateTime.UtcNow.AddHours(7);

			await _context.SaveChangesAsync();
			return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
		}

		// =============================================
		// ✅ DELETE: ลบข้อมูล
		// =============================================
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{

			bool isInUse = await _context.TechStackTags1
				.AnyAsync(x => x.TechId == id); // ⚠️ ตรวจสอบชื่อตัวแปร TechId ใน Model TechStackTag1 อีกทีนะครับ

			if (isInUse)
			{
				return BadRequest(new { Message = "ไม่สามารถลบได้ เนื่องจาก Tech Stack นี้ถูกนำไปใช้งานแล้ว (มีการผูกข้อมูลอยู่ในระบบ)" });
			}

			// 2. ถ้าไม่ถูกใช้งาน ให้ค้นหาและลบที่ "ตารางแม่" (TechStackTags)
			var entity = await _context.TechStackTags.FindAsync(id);

			if (entity == null) return NotFound(new { Message = "ไม่พบข้อมูล Tech Stack" });

			_context.TechStackTags.Remove(entity);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "ลบข้อมูลสำเร็จ" });
		}
	}
}