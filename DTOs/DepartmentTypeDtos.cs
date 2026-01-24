namespace Aimachine.DTOs;

public class CreateDepartmentTypeDto
{
    public string DepartmentTitle { get; set; } = string.Empty;
}

public class UpdateDepartmentTypeDto
{
    public string DepartmentTitle { get; set; } = string.Empty;

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