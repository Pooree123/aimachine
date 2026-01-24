using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Aimachine.DTOs
{
    public class CreateSolutionDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = "Active";

        public List<IFormFile>? ImageFiles { get; set; }
    }
    public class UpdateSolutionDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = "Active";

        public List<IFormFile>? NewImageFiles { get; set; }
    }

    public class SolutionSearchQueryDto
    {
        public string? Q { get; set; }
        public int? DepartmentId { get; set; }

        // ✅ เพิ่มตัวนี้เข้าไปครับ
        public string? DepartmentTitle { get; set; }
    }
}