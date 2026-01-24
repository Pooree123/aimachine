using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateInternDto
    {
        [Required]
        public int JobTitleId { get; set; }
        public string? Description { get; set; }
        public int? TotalPositions { get; set; }
        public DateTime? DateOpen { get; set; }
        public DateTime? DateEnd { get; set; }
        public string? Status { get; set; } = "Active"; // Default Active

        // ✅ รับเป็น List ของ ID เพื่อเอาไปบันทึกลงตาราง InternTag
        public List<int>? StackTagIds { get; set; }
    }

    public class UpdateInternDto
    {
        [Required]
        public int JobTitleId { get; set; }
        public string? Description { get; set; }
        public int? TotalPositions { get; set; }
        public DateTime? DateOpen { get; set; }
        public DateTime? DateEnd { get; set; }
        public string? Status { get; set; } = "Active";

        // ✅ รับ List ใหม่มาทับของเดิม (ถ้าส่งมา)
        public List<int>? StackTagIds { get; set; }
    }

    public class InternSearchQueryDto
    {
        public string? Q { get; set; }
        public int? DepartmentId { get; set; } // dropdown ALL DEPARTMENT
    }
}