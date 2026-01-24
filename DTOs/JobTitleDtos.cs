using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs;

public class CreateJobTitleDto
{
    public int DepartmentId { get; set; }
    public string JobsTitle { get; set; } = string.Empty;
}

public class UpdateJobTitleDto
{
    public int DepartmentId { get; set; }

    public string JobsTitle { get; set; } = string.Empty;
}

public class JobTitleSearchQueryDto
{
    public string? Q { get; set; }
    public int? DepartmentId { get; set; }
}