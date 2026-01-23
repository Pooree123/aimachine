using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateEventCategoryDto
    {
        [Required, MaxLength(100)]
        public string EventTitle { get; set; } = string.Empty;

        public int? CreatedBy { get; set; }
    }

    public class UpdateEventCategoryDto
    {
        [Required, MaxLength(100)]
        public string EventTitle { get; set; } = string.Empty;

        public int? UpdateBy { get; set; }
    }

    public class EventCategoryDropdownDto
    {
        public int Value { get; set; } // เปลี่ยนจาก Id เป็น Value
        public string Label { get; set; } = string.Empty; // เปลี่ยนจาก EventTitle เป็น Label
    }
}
