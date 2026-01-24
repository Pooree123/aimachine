using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs
{
    public class CreateDepartmentTypeDto
    {
        [Required]
        public string DepartmentTitle { get; set; } = string.Empty;
    }

    public class UpdateDepartmentTypeDto
    {
        [Required]
        public string DepartmentTitle { get; set; } = string.Empty;
    }

    public class DepartmentSearchQueryDto
    {
        public string? Q { get; set; }
    }

    public class DepartmentSelectionDto
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}