using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // ✅ จำเป็นต้องมีบรรทัดนี้เพื่อใช้ IFormFile

namespace Aimachine.DTOs
{
    public class CreatePartnerDto
    {
        [MaxLength(255)]
        public string? Image { get; set; } // เอาไว้เก็บชื่อรูป (ถ้าจำเป็น)

        // ✅ เพิ่มตัวนี้เพื่อรับไฟล์รูปภาพจากการอัปโหลด
        public IFormFile? ImageFile { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Status { get; set; } = "Active";

        public int? CreatedBy { get; set; }
    }

    public class UpdatePartnerDto
    {
        [MaxLength(255)]
        public string? Image { get; set; }

        // ✅ เพิ่มตัวนี้เพื่อรับไฟล์รูปภาพใหม่ตอนแก้ไข
        public IFormFile? ImageFile { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Status { get; set; }

        public int? UpdateBy { get; set; }
    }
}