using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class TechStackTag
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public string? ExpertiseTitle { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual DepartmentType Department { get; set; } = null!;

    public virtual ICollection<ExpertiseStatTag> ExpertiseStatTags { get; set; } = new List<ExpertiseStatTag>();

    public virtual ICollection<InternTag> InternTags { get; set; } = new List<InternTag>();

    public virtual ICollection<JobsTag> JobsTags { get; set; } = new List<JobsTag>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
