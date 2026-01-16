using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateInboxDto
    {
        [Required]
        public int TitleId { get; set; }  // topic id

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Phone { get; set; }

        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        // ถ้ายังไม่มีระบบ auth ให้ส่ง adminId มาใน body ก่อน
        public int? CreatedBy { get; set; }
    }

    public class UpdateInboxDto
    {
        [Required]
        public int TitleId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Phone { get; set; }

        [EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        public int? UpdateBy { get; set; }
    }
}
