using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class CompanyProfile
{
    public int Id { get; set; }

    public string? CompannyName { get; set; }

    public string? Description { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? GoogleUrl { get; set; }

    public string? FacebookUrl { get; set; }

    public string? YoutubeUrl { get; set; }

    public string? TiktokUrl { get; set; }

    public string? LineId { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
