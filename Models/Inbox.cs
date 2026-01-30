using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Inbox
{
    public int Id { get; set; }

    public int TitleId { get; set; }

    public string? Name { get; set; }

    public string? Message { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? Deleteflag { get; set; }

    public virtual Topic Title { get; set; } = null!;
}
