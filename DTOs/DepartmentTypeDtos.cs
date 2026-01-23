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

public class DepartmentSearchQueryDto
{
    public string? Q { get; set; }   // keyword search
}

public class DepartmentSelectionDto
{
    public int Value { get; set; }
    public string Label { get; set; } = string.Empty;
}