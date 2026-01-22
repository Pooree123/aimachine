using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Comment
{
    public int Id { get; set; }

    public string? ProfileImg { get; set; }

    public int JobTitleId { get; set; }

    public string? Name { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual JobTitle JobTitle { get; set; } = null!;
}
