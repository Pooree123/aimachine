using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateEventDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(255)]
        public string Events { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Location { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; } = "Active";

        public int? CreatedBy { get; set; }

        // ✅ รองรับ upload หลายรูป
        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateEventDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(255)]
        public string Events { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Location { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public int? UpdateBy { get; set; }
    }

    public class EventSearchQueryDto
    {
        public string? Q { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
    }
}
