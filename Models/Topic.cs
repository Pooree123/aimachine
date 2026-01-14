using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class Topic
{
    public int Id { get; set; }

    public string? TopicTitle { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<Inbox> Inboxes { get; set; } = new List<Inbox>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
