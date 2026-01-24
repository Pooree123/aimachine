using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    // สำหรับ POST (เพิ่มข้อมูล)
    public class CreateTechStackTagDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string TechStackTitle { get; set; } = string.Empty; // ✅ ชื่อใหม่

    }

    // สำหรับ PUT (แก้ไขข้อมูล)
    public class UpdateTechStackTagDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string TechStackTitle { get; set; } = string.Empty; // ✅ ชื่อใหม่

    }

    public class TechStackTagSearchQueryDto
    {
        public string? Q { get; set; }
        public int? DepartmentId { get; set; }
    }
}