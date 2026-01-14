namespace Aimachine.DTOs // เปลี่ยนตามชื่อโปรเจคคุณ
{
    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; } // รับมาเพื่อเอาไป Hash
        public string FullName { get; set; }
    }

    // ใช้สำหรับตอนแก้ไข User
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Status { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}