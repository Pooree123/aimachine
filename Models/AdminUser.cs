using System;
using System.Collections.Generic;

namespace Aimachine.Models;

public partial class AdminUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool? Deleteflag { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<CompanyProfile> CompanyProfiles { get; set; } = new List<CompanyProfile>();

    public virtual AdminUser? CreatedByNavigation { get; set; }

    public virtual ICollection<DepartmentType> DepartmentTypeCreatedByNavigations { get; set; } = new List<DepartmentType>();

    public virtual ICollection<DepartmentType> DepartmentTypeUpdateByNavigations { get; set; } = new List<DepartmentType>();

    public virtual ICollection<EventCategory> EventCategoryCreatedByNavigations { get; set; } = new List<EventCategory>();

    public virtual ICollection<EventCategory> EventCategoryUpdateByNavigations { get; set; } = new List<EventCategory>();

    public virtual ICollection<Event> EventCreatedByNavigations { get; set; } = new List<Event>();

    public virtual ICollection<Event> EventUpdateByNavigations { get; set; } = new List<Event>();

    public virtual ICollection<Inbox> InboxCreatedByNavigations { get; set; } = new List<Inbox>();

    public virtual ICollection<Inbox> InboxUpdateByNavigations { get; set; } = new List<Inbox>();

    public virtual ICollection<Intern> InternCreatedByNavigations { get; set; } = new List<Intern>();

    public virtual ICollection<Intern> InternUpdateByNavigations { get; set; } = new List<Intern>();

    public virtual ICollection<AdminUser> InverseCreatedByNavigation { get; set; } = new List<AdminUser>();

    public virtual ICollection<AdminUser> InverseUpdateByNavigation { get; set; } = new List<AdminUser>();

    public virtual ICollection<Job> JobCreatedByNavigations { get; set; } = new List<Job>();

    public virtual ICollection<JobTitle> JobTitleCreatedByNavigations { get; set; } = new List<JobTitle>();

    public virtual ICollection<JobTitle> JobTitleUpdateByNavigations { get; set; } = new List<JobTitle>();

    public virtual ICollection<Job> JobUpdateByNavigations { get; set; } = new List<Job>();

    public virtual ICollection<Partner> PartnerCreatedByNavigations { get; set; } = new List<Partner>();

    public virtual ICollection<Partner> PartnerUpdateByNavigations { get; set; } = new List<Partner>();

    public virtual ICollection<Solution> SolutionCreatedByNavigations { get; set; } = new List<Solution>();

    public virtual ICollection<Solution> SolutionUpdateByNavigations { get; set; } = new List<Solution>();

    public virtual ICollection<TechStack> TechStackCreatedByNavigations { get; set; } = new List<TechStack>();

    public virtual ICollection<TechStackTag> TechStackTagCreatedByNavigations { get; set; } = new List<TechStackTag>();

    public virtual ICollection<TechStackTag> TechStackTagUpdateByNavigations { get; set; } = new List<TechStackTag>();

    public virtual ICollection<TechStack> TechStackUpdateByNavigations { get; set; } = new List<TechStack>();

    public virtual ICollection<Topic> TopicCreatedByNavigations { get; set; } = new List<Topic>();

    public virtual ICollection<Topic> TopicUpdateByNavigations { get; set; } = new List<Topic>();

    public virtual AdminUser? UpdateByNavigation { get; set; }
}
