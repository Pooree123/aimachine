using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreatePartnerDto
    {
        [MaxLength(255)]
        public string? Image { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Status { get; set; } = "Active";

        public int? CreatedBy { get; set; }
    }

    public class UpdatePartnerDto
    {
        [MaxLength(255)]
        public string? Image { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Status { get; set; }

        public int? UpdateBy { get; set; }
    }
}
