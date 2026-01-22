using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateEventCategoryDto
    {
        [Required, MaxLength(100)]
        public string EventTitle { get; set; } = string.Empty;

        public int? CreatedBy { get; set; }

        public string? Status { get; set; } = "Active"; // ถ้าตารางมี status
    }

    public class UpdateEventCategoryDto
    {
        [Required, MaxLength(100)]
        public string EventTitle { get; set; } = string.Empty;

        public int? UpdateBy { get; set; }

        public string? Status { get; set; } // ถ้าตารางมี status
    }
}
