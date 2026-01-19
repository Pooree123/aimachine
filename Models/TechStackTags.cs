using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class TechStackTag1
{
    public int Id { get; set; }

    public int? TechId { get; set; }

    public int? StackTagId { get; set; }

    public virtual TechStackTag? StackTag { get; set; }

    public virtual TechStack? Tech { get; set; }
}
