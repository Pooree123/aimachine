using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class EventsImg
{
    public int Id { get; set; }

    public int EventsId { get; set; }

    public string? Image { get; set; }

    public bool? IsCover { get; set; }

    public int? OrderId { get; set; }

    public virtual Event Events { get; set; } = null!;
}
