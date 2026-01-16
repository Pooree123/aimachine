namespace Aimachine.DTOs;

public class CreateDepartmentTypeDto
{
    public string DepartmentTitle { get; set; } = string.Empty;
    public int? CreatedBy { get; set; }
}

public class UpdateDepartmentTypeDto
{
    public string DepartmentTitle { get; set; } = string.Empty;
    public int? UpdateBy { get; set; }
}
