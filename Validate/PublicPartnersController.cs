using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Controllers
{
    [ApiController]
    [Route("api/public/partners")]
    public class PublicPartnersController : ControllerBase
    {
        private readonly AimachineContext _context;
        public PublicPartnersController(AimachineContext context) => _context = context;

        // GET: /api/public/partners
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.Partners
                    .AsNoTracking()
                    .Where(p => p.Status == "Active")   // หน้า public โชว์เฉพาะ Active
                    .OrderByDescending(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Image,
                        p.Status
                    })
                    .ToListAsync();

                return Ok(new { Message = "ดึงข้อมูลสำเร็จ", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "ดึงข้อมูลไม่สำเร็จ", Error = ex.Message });
            }
        }
    }
}
