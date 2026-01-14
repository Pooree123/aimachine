using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class SolutionImg
{
    public int Id { get; set; }

    public int SolutionId { get; set; }

    public string? Image { get; set; }

    public bool? IsCover { get; set; }

    public int? OrderId { get; set; }

    public virtual Solution Solution { get; set; } = null!;
}
