using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class DepartmentType
{
    public int Id { get; set; }

    public string? DepartmentTitle { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? CanDelete { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<JobTitle> JobTitles { get; set; } = new List<JobTitle>();

    public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();

    public virtual ICollection<TechStackTag> TechStackTags { get; set; } = new List<TechStackTag>();

    public virtual AdminUser? UpdateByNavigation { get; set; }

    public virtual ICollection<Partner> Partners { get; set; } = new List<Partner>();
}
