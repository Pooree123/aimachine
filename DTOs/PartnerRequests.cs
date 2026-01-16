using Microsoft.AspNetCore.Http; // ต้องมีบรรทัดนี้เพื่อใช้ IFormFile

namespace Aimachine.DTOs
{
    // สำหรับตอนสร้าง (POST)
    public class CreatePartnerDto
    {
        public string Name { get; set; }
        public IFormFile ImageFile { get; set; } // รับไฟล์รูป
        public string Status { get; set; }
    }

    // สำหรับตอนแก้ไข (PUT)
    public class UpdatePartnerDto
    {
        public string Name { get; set; }
        public IFormFile? ImageFile { get; set; } // ใส่ ? คือไม่ส่งรูปใหม่มาก็ได้ (ใช้รูปเดิม)
        public string Status { get; set; }
    }
}