using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class JobTitle
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public string? JobsTitle { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual DepartmentType Department { get; set; } = null!;

    public virtual ICollection<Intern> Interns { get; set; } = new List<Intern>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
