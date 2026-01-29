using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Aimachine.Models;

public partial class AimachineContext : DbContext
{
    public AimachineContext()
    {
    }

    public AimachineContext(DbContextOptions<AimachineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<CompanyProfile> CompanyProfiles { get; set; }
    public virtual DbSet<DepartmentType> DepartmentTypes { get; set; }
    public virtual DbSet<Event> Events { get; set; }
    public virtual DbSet<EventCategory> EventCategories { get; set; }
    public virtual DbSet<EventsImg> EventsImgs { get; set; }
    public virtual DbSet<Inbox> Inboxes { get; set; }
    public virtual DbSet<Intern> Interns { get; set; }
    public virtual DbSet<InternTag> InternTags { get; set; }
    public virtual DbSet<Job> Jobs { get; set; }
    public virtual DbSet<JobTitle> JobTitles { get; set; }
    public virtual DbSet<JobsTag> JobsTags { get; set; }
    public virtual DbSet<Partner> Partners { get; set; }
    public virtual DbSet<Solution> Solutions { get; set; }
    public virtual DbSet<SolutionImg> SolutionImgs { get; set; }
    public virtual DbSet<TechStack> TechStacks { get; set; }
    public virtual DbSet<TechStackTag> TechStackTags { get; set; }
    public virtual DbSet<TechStackTag1> TechStackTags1 { get; set; }
    public virtual DbSet<Topic> Topics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("admin_users");
            entity.HasIndex(e => e.Username).IsUnique(); // PostgreSQL ใช้ชื่อ Index อัตโนมัติได้

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Deleteflag).HasDefaultValue(false).HasColumnName("deleteflag");
            entity.Property(e => e.FullName).HasMaxLength(100).HasColumnName("full_name");
            entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
            entity.Property(e => e.Status).HasMaxLength(255).HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            entity.Property(e => e.Username).HasMaxLength(50).HasColumnName("username");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.InverseUpdateByNavigation).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.JobTitleId).HasColumnName("job_title_id");
            entity.Property(e => e.Message).HasMaxLength(250).HasColumnName("message");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.ProfileImg).HasMaxLength(255).HasColumnName("profile_img");
            entity.Property(e => e.Status).HasMaxLength(255).HasDefaultValue("Active").HasColumnName("status");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.Comments).HasForeignKey(d => d.JobTitleId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.ToTable("company_profile");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasMaxLength(255).HasColumnName("address");
            entity.Property(e => e.CompannyName).HasMaxLength(100).HasColumnName("companny_name");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.FacebookUrl).HasMaxLength(255).HasColumnName("facebook_url");
            entity.Property(e => e.GoogleUrl).HasMaxLength(255).HasColumnName("google_url");
            entity.Property(e => e.YoutubeUrl).HasMaxLength(255).HasColumnName("youtube_url");
            entity.Property(e => e.TiktokUrl).HasMaxLength(255).HasColumnName("tiktok_url");
            entity.Property(e => e.LineId).HasMaxLength(50).HasColumnName("line_id");
            entity.Property(e => e.Phone).HasMaxLength(10).HasColumnName("phone");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.CompanyProfiles).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<DepartmentType>(entity =>
        {
            entity.ToTable("department_types");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CanDelete).HasColumnName("can_delete");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentTitle).HasMaxLength(100).HasColumnName("department_title");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DepartmentTypeCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.DepartmentTypeUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.EventDate).HasColumnName("event_date").HasColumnType("timestamp without time zone");
            entity.Property(e => e.Events).HasMaxLength(255).HasColumnName("events");
            entity.Property(e => e.Location).HasMaxLength(255).HasColumnName("location");
            entity.Property(e => e.Status).HasMaxLength(255).HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.Category).WithMany(p => p.Events).HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EventCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.EventUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.ToTable("event_categories");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EventTitle).HasMaxLength(100).HasColumnName("event_title");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EventCategoryCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.EventCategoryUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<EventsImg>(entity =>
        {
            entity.ToTable("events_img");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventsId).HasColumnName("events_id");
            entity.Property(e => e.Image).HasMaxLength(255).HasColumnName("image");
            entity.Property(e => e.IsCover).HasDefaultValue(false).HasColumnName("isCover");
            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.HasOne(d => d.Events).WithMany(p => p.EventsImgs).HasForeignKey(d => d.EventsId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Inbox>(entity =>
        {
            entity.ToTable("inbox");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Deleteflag).HasDefaultValue(false).HasColumnName("deleteflag");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.Message).HasMaxLength(300).HasColumnName("message");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Phone).HasMaxLength(10).HasColumnName("phone");
            entity.Property(e => e.TitleId).HasColumnName("title_id");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InboxCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.Title).WithMany(p => p.Inboxes).HasForeignKey(d => d.TitleId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.InboxUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<Intern>(entity =>
        {
            entity.ToTable("interns");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateEnd).HasColumnName("date_end").HasColumnType("timestamp without time zone");
            entity.Property(e => e.DateOpen).HasColumnName("date_open").HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.JobTitleId).HasColumnName("job_title_id");
            entity.Property(e => e.Status).HasMaxLength(255).HasColumnName("status");
            entity.Property(e => e.TotalPositions).HasColumnName("total_positions");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InternCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.JobTitle).WithMany(p => p.Interns).HasForeignKey(d => d.JobTitleId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.InternUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<InternTag>(entity =>
        {
            entity.ToTable("intern_tags");
            entity.HasIndex(e => new { e.InternId, e.StackTagId }).IsUnique();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InternId).HasColumnName("intern_id");
            entity.Property(e => e.StackTagId).HasColumnName("stack_tag_id");

            entity.HasOne(d => d.Intern).WithMany(p => p.InternTags).HasForeignKey(d => d.InternId);
            entity.HasOne(d => d.StackTag).WithMany(p => p.InternTags).HasForeignKey(d => d.StackTagId);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateEnd).HasColumnName("date_end").HasColumnType("timestamp without time zone");
            entity.Property(e => e.DateOpen).HasColumnName("date_open").HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.JobTitleId).HasColumnName("job_title_id");
            entity.Property(e => e.Status).HasMaxLength(255).HasColumnName("status");
            entity.Property(e => e.TotalPositions).HasColumnName("total_positions");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.JobTitle).WithMany(p => p.Jobs).HasForeignKey(d => d.JobTitleId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.JobUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<JobTitle>(entity =>
        {
            entity.ToTable("job_title");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.JobsTitle).HasMaxLength(255).HasColumnName("jobs_title");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobTitleCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.Department).WithMany(p => p.JobTitles).HasForeignKey(d => d.DepartmentId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.JobTitleUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<JobsTag>(entity =>
        {
            entity.ToTable("jobs_tags");
            entity.HasIndex(e => new { e.JobId, e.StackTagId }).IsUnique();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.StackTagId).HasColumnName("stack_tag_id");

            entity.HasOne(d => d.Job).WithMany(p => p.JobsTags).HasForeignKey(d => d.JobId);
            entity.HasOne(d => d.StackTag).WithMany(p => p.JobsTags).HasForeignKey(d => d.StackTagId);
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("partners");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Image).HasMaxLength(255).HasColumnName("image");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Status).HasMaxLength(255).HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PartnerCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.PartnerUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
            entity.HasOne(d => d.Department).WithMany(p => p.Partners).HasForeignKey(d => d.DepartmentId);
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.ToTable("solutions");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Status).HasMaxLength(255).HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SolutionCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.Department).WithMany(p => p.Solutions).HasForeignKey(d => d.DepartmentId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.SolutionUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<SolutionImg>(entity =>
        {
            entity.ToTable("solution_img");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Image).HasMaxLength(255).HasColumnName("image");
            entity.Property(e => e.IsCover).HasDefaultValue(false).HasColumnName("isCover");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.SolutionId).HasColumnName("solution_id");

            entity.HasOne(d => d.Solution).WithMany(p => p.SolutionImgs).HasForeignKey(d => d.SolutionId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TechStack>(entity =>
        {
            entity.ToTable("tech_stacks");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.Header).HasMaxLength(100).HasColumnName("header");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TechStackCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.TechStackUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<TechStackTag>(entity =>
        {
            entity.ToTable("tech_stack_tag");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.TechStackTitle).HasMaxLength(100).HasColumnName("tech_stack_title");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TechStackTagCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.Department).WithMany(p => p.TechStackTags).HasForeignKey(d => d.DepartmentId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.TechStackTagUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        modelBuilder.Entity<TechStackTag1>(entity =>
        {
            entity.ToTable("tech_stack_tags");
            entity.HasIndex(e => new { e.TechId, e.StackTagId }).IsUnique();
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StackTagId).HasColumnName("stack_tag_id");
            entity.Property(e => e.TechId).HasColumnName("tech_id");

            entity.HasOne(d => d.StackTag).WithMany(p => p.TechStackTag1s).HasForeignKey(d => d.StackTagId);
            entity.HasOne(d => d.Tech).WithMany(p => p.TechStackTag1s).HasForeignKey(d => d.TechId);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("topic");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.TopicTitle).HasMaxLength(100).HasColumnName("topic_title");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TopicCreatedByNavigations).HasForeignKey(d => d.CreatedBy);
            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.TopicUpdateByNavigations).HasForeignKey(d => d.UpdateBy);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}