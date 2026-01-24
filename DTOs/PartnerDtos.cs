using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Aimachine.DTOs
{
    public class CreatePartnerDto
    {
        [MaxLength(255)]
        public string? Image { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        [MaxLength(255)]
        public string? Status { get; set; } = "Active";

        public int? CreatedBy { get; set; }
    }

    public class UpdatePartnerDto
    {
        [MaxLength(255)]
        public string? Image { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        [MaxLength(255)]
        public string? Status { get; set; }

        public int? UpdateBy { get; set; }
    }

    public class PartnerSearchQueryDto
    {
        public string? Q { get; set; }
    }
}