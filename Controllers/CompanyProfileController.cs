using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers;

[ApiController]
[Route("api/companyprofile")]
public class CompanyProfileController : ControllerBase
{
    private readonly AimachineContext _context;
    public CompanyProfileController(AimachineContext context) => _context = context;

    // GET: api/companyprofile
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // แก้ไข: เพิ่ม Where(x => x.Id == 1) เพื่อดึงเฉพาะ ID 1
        var cp = await _context.CompanyProfiles.AsNoTracking()
          .Where(x => x.Id == 1)
          .Select(x => new {
              x.Id,
              x.CompannyName,
              x.Description,
              x.Email,
              x.Phone,
              x.Address,
              x.GoogleUrl,
              x.FacebookUrl,
              x.LineId,
              x.UpdateBy,
              x.UpdateAt
          })
          .FirstOrDefaultAsync();

        if (cp == null) return NotFound(new { Message = "ไม่พบข้อมูล company_profile (ID 1)" });
        return Ok(cp);
    }

    // PUT: api/companyprofile
    // แก้ไข: เอา {id} ออกจาก Route เพราะเราจะ fix เป็น 1 ข้างใน
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCompanyProfileDto dto)
    {
        try
        {
            // แก้ไข: Fix ID เป็น 1 เสมอ
            int fixedId = 1;
            var cp = await _context.CompanyProfiles.FindAsync(fixedId);

            if (cp == null) return NotFound(new { Message = "ไม่พบข้อมูล company_profile (ID 1)" });

            if (dto.UpdateBy.HasValue && !await _context.AdminUsers.AnyAsync(a => a.Id == dto.UpdateBy.Value))
                return BadRequest(new { Message = "update_by ไม่ถูกต้อง (ไม่พบ admin_users)" });

            cp.CompannyName = dto.CompannyName?.Trim();
            cp.Description = dto.Description?.Trim();
            cp.Email = dto.Email?.Trim();
            cp.Phone = dto.Phone?.Trim();
            cp.Address = dto.Address?.Trim();
            cp.GoogleUrl = dto.GoogleUrl?.Trim();
            cp.FacebookUrl = dto.FacebookUrl?.Trim();
            cp.LineId = dto.LineId?.Trim();
            cp.UpdateBy = dto.UpdateBy;
            cp.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.InnerException?.Message ?? ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "แก้ไขข้อมูลไม่สำเร็จ", Error = ex.Message });
        }
    }
}