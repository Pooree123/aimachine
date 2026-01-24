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

            cp.CompannyName = dto.CompannyName?.Trim();
            cp.Description = dto.Description?.Trim();
            cp.Email = dto.Email?.Trim();

            // ---------------------------------------------------------
            // ✅ แก้ไขส่วน Phone: ตัดขีด/สัญลักษณ์ออก เอาแค่เลข 10 หลัก
            // ---------------------------------------------------------
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                // 1. กรองเอาเฉพาะตัวเลข (0-9) ตัด - หรือ space หรือ () ออกหมด
                string cleanedPhone = new string(dto.Phone.Where(char.IsDigit).ToArray());

                // 2. ตรวจสอบว่าต้องมี 10 หลักเท่านั้น (ถ้าไม่ครบ หรือเกิน ให้แจ้ง Error)
                if (cleanedPhone.Length != 10)
                {
                    return BadRequest(new { Message = "เบอร์โทรศัพท์ไม่ถูกต้อง กรุณาระบุตัวเลข 10 หลัก" });
                }

                cp.Phone = cleanedPhone;
            }
            else
            {
                cp.Phone = null; // กรณีส่งมาเป็นค่าว่าง ให้เคลียร์ค่าใน DB
            }
            // ---------------------------------------------------------

            cp.Address = dto.Address?.Trim();
            cp.GoogleUrl = dto.GoogleUrl?.Trim();
            cp.FacebookUrl = dto.FacebookUrl?.Trim();

            // ✅ เพิ่มการอัปเดต 2 ค่านี้
            cp.YoutubeUrl = dto.YoutubeUrl?.Trim();
            cp.TiktokUrl = dto.TiktokUrl?.Trim();

            cp.LineId = dto.LineId?.Trim();

            cp.UpdateBy = currentUserId;
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