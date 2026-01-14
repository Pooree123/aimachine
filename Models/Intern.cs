using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Intern
{
    public int Id { get; set; }

    public int JobTitleId { get; set; }

    public string? Description { get; set; }

    public int? TotalPositions { get; set; }

    public DateTime? DateOpen { get; set; }

    public DateTime? DateEnd { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<InternTag> InternTags { get; set; } = new List<InternTag>();

    public virtual JobTitle JobTitle { get; set; } = null!;

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
