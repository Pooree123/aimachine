using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateTopicDto
    {
        [Required, MaxLength(100)]
        public string TopicTitle { get; set; } = string.Empty;

        public int? CreatedBy { get; set; } // ยังไม่มี auth ก็ส่งมาได้
    }

    public class UpdateTopicDto
    {
        [Required, MaxLength(100)]
        public string TopicTitle { get; set; } = string.Empty;

        public int? UpdateBy { get; set; }
    }

    public class TopicSearchQueryDto
    {
        public string? Q { get; set; }
    }

    public class TopicDropdownDto
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
