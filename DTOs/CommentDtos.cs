using Microsoft.AspNetCore.Http; // อย่าลืม using นี้
using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateCommentDto
    {
        public int JobTitleId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }

        public string? ProfileImg { get; set; }
    }
}