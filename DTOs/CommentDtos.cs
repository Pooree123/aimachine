namespace Aimachine.DTOs;

public class CreateCommentDto
{
    public int JobTitleId { get; set; }
    public string? ProfileImg { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
