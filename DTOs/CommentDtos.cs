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
}