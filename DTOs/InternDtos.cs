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
        public string Status { get; set; } = "Active";
        public int CreatedBy { get; set; }

        // ✅ รับเป็น List ของ ID (เช่น [1, 5, 8]) เพื่อเอาไปบันทึกลงตาราง InternTag
        public List<int> StackTagIds { get; set; } = new List<int>();
    }

    public class UpdateInternDto
    {
        [Required]
        public int JobTitleId { get; set; }
        public string? Description { get; set; }
        public int? TotalPositions { get; set; }
        public DateTime? DateOpen { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Status { get; set; } = "Active";
        public int UpdateBy { get; set; }

        // ✅ รับ List ใหม่มาทับของเดิม
        public List<int> StackTagIds { get; set; } = new List<int>();
    }
}