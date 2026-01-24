using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateEventCategoryDto
    {
        [Required, MaxLength(100)]
        public string EventTitle { get; set; } = string.Empty;
    }

    public class UpdateEventCategoryDto
    {
        [Required, MaxLength(100)]
        public string EventTitle { get; set; } = string.Empty;
    }

    public class EventCategorySearchQueryDto
    {
        public string? Q { get; set; }
    }

    public class EventCategoryDropdownDto
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}