namespace Aimachine.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;

        // ✅ เพิ่ม CreatedBy สำหรับเก็บ ID คนสร้าง
        public int CreatedBy { get; set; }
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = null!;
        public string Status { get; set; } = null!;

        // ✅ เพิ่ม UpdateBy สำหรับเก็บ ID คนแก้ไข
        public int UpdateBy { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}