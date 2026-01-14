using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using Aimachine.DTOs;
using Aimachine.Models; // อย่าลืมเปลี่ยนเป็น namespace ของคุณ

namespace Aimachine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AimachineContext _context;

        public UsersController(AimachineContext context)
        {
            _context = context;
        }

        // 1. GET: ดึงข้อมูล User ทั้งหมด (ไม่เอาคนทีโดนลบ)
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.AdminUsers
                .Where(u => u.Deleteflag == false) // กรองคนที่โดนลบออก
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Status,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. GET: ดึงข้อมูลตาม ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.AdminUsers
                .Where(u => u.Id == id && u.Deleteflag == false)
                .Select(u => new { u.Id, u.Username, u.FullName, u.Status })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("ไม่พบผู้ใช้งานนี้");

            return Ok(user);
        }

        // 3. POST: สร้าง User ใหม่
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {
            // 1. เช็คว่า Username ซ้ำไหม
            if (await _context.AdminUsers.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username นี้มีคนใช้แล้ว");
            }

            // 2. Hash Password (สำคัญมาก!)
            // ใช้ตัวแปรนี้ไปบันทึกลง Database แทนรหัสจริง
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. สร้าง Object ลง DB
            var newUser = new AdminUser
            {
                Username = request.Username,
                PasswordHash = passwordHash, // เก็บตัวที่ Hash แล้ว
                FullName = request.FullName,
                Status = "Active", // ค่าเริ่มต้น
                Deleteflag = false,
                CreatedBy = 1, // สมมติใส่ ID Admin ที่สร้างไปก่อน
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdateAt = DateTime.UtcNow.AddHours(7)
            };

            _context.AdminUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "สร้าง User สำเร็จ", UserId = newUser.Id });
        }

        // 4. PUT: แก้ไขข้อมูล
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = request.FullName;
            user.Status = request.Status;
            user.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

        // 5. DELETE: ลบ User (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null) return NotFound();

            // เราไม่ลบจริง แต่แค่แปะป้ายว่าลบแล้ว (Soft Delete)
            user.Deleteflag = true;
            user.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "ลบผู้ใช้งานสำเร็จ (Soft Delete)" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. ค้นหา User จาก Username
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Deleteflag == false);

            // 2. ถ้าไม่เจอ User
            if (user == null)
            {
                return Unauthorized("ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
            }

            // 3. ตรวจสอบรหัสผ่าน (เอาที่กรอก มาเทียบกับ Hash ใน DB)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
            }

            // 4. ถ้าผ่านหมด (Login สำเร็จ)
            return Ok(new
            {
                Message = "เข้าสู่ระบบสำเร็จ",
                UserId = user.Id,
                FullName = user.FullName
            });
        }
    }
}