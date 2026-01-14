using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class JobsTag
{
    public int Id { get; set; }

    public int? JobId { get; set; }

    public int? StackTagId { get; set; }

    public virtual Job? Job { get; set; }

    public virtual TechStackTag? StackTag { get; set; }
}
