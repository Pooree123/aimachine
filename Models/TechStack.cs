using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class TechStack
{
    public int Id { get; set; }

    public string? Header { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<ExpertiseStatTag> ExpertiseStatTags { get; set; } = new List<ExpertiseStatTag>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
