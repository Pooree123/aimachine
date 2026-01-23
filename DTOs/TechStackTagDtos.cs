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

        public int CreatedBy { get; set; }
    }

    // สำหรับ PUT (แก้ไขข้อมูล)
    public class UpdateTechStackTagDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string TechStackTitle { get; set; } = string.Empty; // ✅ ชื่อใหม่

        public int UpdateBy { get; set; }
    }

    public class TechStackTagSearchQueryDto
    {
        public string? Q { get; set; }
        public int? DepartmentId { get; set; }
    }
}