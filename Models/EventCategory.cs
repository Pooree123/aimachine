using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class EventCategory
{
    public int Id { get; set; }

    public string? EventTitle { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
