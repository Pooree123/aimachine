using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;

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
        // ดึงเฉพาะ ID 1
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
              // ✅ เพิ่ม 2 บรรทัดนี้ใน Select
              x.YoutubeUrl,
              x.TiktokUrl,
              x.LineId,
              x.UpdateBy,
              x.UpdateAt
          })
          .FirstOrDefaultAsync();

        if (cp == null) return NotFound(new { Message = "ไม่พบข้อมูล company_profile (ID 1)" });
        return Ok(cp);
    }

    // PUT: api/companyprofile
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateCompanyProfileDto dto)
    {
        int currentUserId = User.GetUserId();

        try
        {
            int fixedId = 1;
            var cp = await _context.CompanyProfiles.FindAsync(fixedId);

            if (cp == null) return NotFound(new { Message = "ไม่พบข้อมูล company_profile (ID 1)" });

            // (ส่วนเช็ค AdminUsers เอาออกก็ได้ถ้าไม่ได้ใช้ หรือถ้าใช้ก็เก็บไว้)
            // if (dto.UpdateBy.HasValue && !await _context.AdminUsers.AnyAsync(a => a.Id == dto.UpdateBy.Value))
            //     return BadRequest(new { Message = "update_by ไม่ถูกต้อง" });

            cp.CompannyName = dto.CompannyName?.Trim();
            cp.Description = dto.Description?.Trim();
            cp.Email = dto.Email?.Trim();
            cp.Phone = dto.Phone?.Trim();
            cp.Address = dto.Address?.Trim();
            cp.GoogleUrl = dto.GoogleUrl?.Trim();
            cp.FacebookUrl = dto.FacebookUrl?.Trim();

            // ✅ เพิ่มการอัปเดต 2 ค่านี้
            cp.YoutubeUrl = dto.YoutubeUrl?.Trim();
            cp.TiktokUrl = dto.TiktokUrl?.Trim();

            cp.LineId = dto.LineId?.Trim();

            cp.UpdateBy = currentUserId; // ใช้ ID จาก Token เสมอ
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