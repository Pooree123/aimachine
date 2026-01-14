using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Event
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string? Events { get; set; }

    public string? Location { get; set; }

    public DateTime? EventDate { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual EventCategory Category { get; set; } = null!;

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<EventsImg> EventsImgs { get; set; } = new List<EventsImg>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
