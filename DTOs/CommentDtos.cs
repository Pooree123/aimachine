using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        public int JobTitleId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; } // ใน DB เป็น NULL ได้ เลยใส่ ? ไว้

        [MaxLength(250)]
        public string? Message { get; set; } // ใน DB เป็น NULL ได้

        public IFormFile? ImageFile { get; set; }

        public string? ProfileImg { get; set; }
    }

    public class CommentSearchQueryDto
    {
        public string? Q { get; set; }
        public DateTime? Date { get; set; }
        public int? JobTitleId { get; set; } // ✅ filter จาก dropdown
    }

    public class UpdateCommentStatusDto
    {
        [Required]
        public string Status { get; set; } = null!; // ค่าที่ส่งมาเช่น "Active" หรือ "inActive"
    }
}