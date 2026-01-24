using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateJobDto
    {
        // ❌ เอา DepartmentId ออก เพราะใน Model Job ไม่มี

        [Required]
        public int JobTitleId { get; set; }

        public string? Description { get; set; }

        // ✅ เพิ่มฟิลด์ที่มีจริงใน Model
        public int? TotalPositions { get; set; } // รับจำนวนอัตรา
        public DateTime? DateOpen { get; set; }  // วันที่เปิดรับ
        public DateTime? DateEnd { get; set; }   // วันที่ปิดรับ

        public string Status { get; set; } = "Active";
        public List<int>? TechStackTagIds { get; set; }
    }

    public class UpdateJobDto
    {
        // ❌ เอา DepartmentId ออก

        [Required]
        public int JobTitleId { get; set; }

        public string? Description { get; set; }

        public int? TotalPositions { get; set; }
        public DateTime? DateOpen { get; set; }
        public DateTime? DateEnd { get; set; }

        public string Status { get; set; } = "Active";

        public List<int>? TechStackTagIds { get; set; }
    }

    public class JobSearchQueryDto
    {
        public string? Q { get; set; }
        public int? JobTitleId { get; set; }
        public string? Status { get; set; }
    }
}