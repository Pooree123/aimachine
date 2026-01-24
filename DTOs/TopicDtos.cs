using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateTopicDto
    {
        [Required, MaxLength(100)]
        public string TopicTitle { get; set; } = string.Empty;
    }

    public class UpdateTopicDto
    {
        [Required, MaxLength(100)]
        public string TopicTitle { get; set; } = string.Empty;
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