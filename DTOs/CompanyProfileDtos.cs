namespace Aimachine.DTOs;

public class UpdateCompanyProfileDto
{
    public string? CompannyName { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? GoogleUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LineId { get; set; }
    public int? UpdateBy { get; set; }
}
