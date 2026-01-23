using System.ComponentModel.DataAnnotations;

namespace Aimachine.DTOs;

public class CreateJobTitleDto
{
    public int DepartmentId { get; set; }
    public string JobsTitle { get; set; } = string.Empty;
    public int? CreatedBy { get; set; }
}

public class UpdateJobTitleDto
{
    public string JobsTitle { get; set; } = string.Empty;
    public int? UpdateBy { get; set; }
}

public class JobTitleSearchQueryDto
{
    public string? Q { get; set; }
    public int? DepartmentId { get; set; }
}
