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
}
