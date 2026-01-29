using Aimachine.DTOs;
using Aimachine.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Aimachine.Extensions;
using System.Text.RegularExpressions;

namespace Aimachine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AimachineContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(AimachineContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ✅ Helper Check: Username ต้องเป็น A-Z, a-z, 0-9 เท่านั้น
        private bool IsValidUsername(string username)
        {
            // Regex: ^ เริ่มต้น, [a-zA-Z0-9] ตัวอักษรหรือตัวเลข, + มี 1 ตัวขึ้นไป, $ จบประโยค
            return Regex.IsMatch(username, "^[a-zA-Z0-9]+$");
        }

        // ✅ Helper Check: Password Complexity
        private bool IsPasswordStrong(string password, out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "รหัสผ่านห้ามเป็นค่าว่าง";
                return false;
            }
            if (password.Length < 8)
            {
                errorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 8 ตัวอักษร";
                return false;
            }
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "รหัสผ่านต้องมีตัวพิมพ์ใหญ่ (A-Z) อย่างน้อย 1 ตัว";
                return false;
            }
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "รหัสผ่านต้องมีตัวพิมพ์เล็ก (a-z) อย่างน้อย 1 ตัว";
                return false;
            }
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "รหัสผ่านต้องมีตัวเลข (0-9) อย่างน้อย 1 ตัว";
                return false;
            }
            return true;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.AdminUsers
                .Where(u => u.Deleteflag == false)
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

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.AdminUsers
                .Where(u => u.Id == id && u.Deleteflag == false)
                .Select(u => new { u.Id, u.Username, u.FullName, u.Status })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("ไม่พบผู้ใช้งานนี้");

            return Ok(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {
            int currentUserId = User.GetUserId();
            var username = request.Username.Trim();

            // 1. ✅ เช็คอักขระพิเศษใน Username
            if (!IsValidUsername(username))
            {
                return BadRequest(new { Message = "Username ต้องประกอบด้วยตัวอักษรภาษาอังกฤษ (a-z) หรือตัวเลข (0-9) เท่านั้น ห้ามมีช่องว่างหรืออักขระพิเศษ" });
            }

            // 2. ✅ เช็ค Username ซ้ำ
            if (await _context.AdminUsers.AnyAsync(u => u.Username.ToLower() == username.ToLower() && u.Deleteflag == false))
            {
                return BadRequest(new { Message = $"Username '{username}' มีผู้ใช้งานแล้ว" });
            }

            // 3. ✅ เช็คความยากรหัสผ่าน
            if (!IsPasswordStrong(request.Password, out string pwdError))
            {
                return BadRequest(new { Message = pwdError });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new AdminUser
            {
                Username = username,
                PasswordHash = passwordHash,
                FullName = request.FullName?.Trim(),
                Status = "Active",
                Deleteflag = false,
                CreatedBy = currentUserId,
                UpdateBy = currentUserId,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdateAt = DateTime.UtcNow.AddHours(7)
            };

            _context.AdminUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "สร้าง User สำเร็จ", UserId = newUser.Id });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            int currentUserId = User.GetUserId();
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null) return NotFound();

            var newUsername = request.Username.Trim();

            // 1. ✅ เช็คอักขระพิเศษใน Username (กรณีมีการเปลี่ยนชื่อ)
            if (!IsValidUsername(newUsername))
            {
                return BadRequest(new { Message = "Username ต้องประกอบด้วยตัวอักษรภาษาอังกฤษ (a-z) หรือตัวเลข (0-9) เท่านั้น" });
            }

            // 2. ✅ เช็ค Username ซ้ำ (เฉพาะถ้าเปลี่ยนชื่อใหม่)
            if (newUsername.ToLower() != user.Username.ToLower())
            {
                var exists = await _context.AdminUsers
                    .AnyAsync(u => u.Username.ToLower() == newUsername.ToLower() && u.Id != id && u.Deleteflag == false);

                if (exists)
                    return BadRequest(new { Message = $"Username '{newUsername}' มีผู้ใช้งานแล้ว" });
            }

            user.FullName = request.FullName?.Trim();
            user.Username = newUsername;
            user.UpdateBy = currentUserId;
            user.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int currentUserId = User.GetUserId();

            if (id == currentUserId)
            {
                return BadRequest(new { Message = "ไม่สามารถลบบัญชีของตัวเองได้" });
            }

            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null) return NotFound(new { Message = "ไม่พบผู้ใช้งาน" });

            user.Deleteflag = true;
            user.UpdateAt = DateTime.UtcNow.AddHours(7);
            user.UpdateBy = currentUserId; 

            await _context.SaveChangesAsync();
            return Ok(new { Message = "ลบผู้ใช้งานสำเร็จ (Soft Delete)" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Deleteflag == false);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.FullName ?? "")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Message = "เข้าสู่ระบบสำเร็จ",
                Token = tokenString,
            });
        }
    }
}