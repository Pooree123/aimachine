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

            if (await _context.AdminUsers.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username นี้มีคนใช้แล้ว");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new AdminUser
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                FullName = request.FullName,
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

            user.FullName = request.FullName;
            user.Username = request.Username;

            user.UpdateBy = currentUserId;

            user.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "แก้ไขข้อมูลสำเร็จ" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null) return NotFound();

            user.Deleteflag = true;
            user.UpdateAt = DateTime.UtcNow.AddHours(7);

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
                return Unauthorized("ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
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