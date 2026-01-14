using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class InternTag
{
    public int Id { get; set; }

    public int? InternId { get; set; }

    public int? StackTagId { get; set; }

    public virtual Intern? Intern { get; set; }

    public virtual TechStackTag? StackTag { get; set; }
}
