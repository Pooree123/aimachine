using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Solution
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual DepartmentType Department { get; set; } = null!;

    public virtual ICollection<SolutionImg> SolutionImgs { get; set; } = new List<SolutionImg>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
