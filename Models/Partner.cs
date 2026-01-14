using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Partner
{
    public int Id { get; set; }

    public string? Image { get; set; }

    public string? Name { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
