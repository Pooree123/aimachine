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

    // ✅ แก้ไขส่วนนี้: ลบ #warning และใส่เงื่อนไข IsConfigured
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // ใส่ไว้เป็น Fallback เผื่อรันแบบไม่มี Program.cs (แต่ปกติจะอ่านจาก appsettings.json)
            optionsBuilder.UseSqlServer("Server=DESKTOP-BL4MRCB\\SQLEXPRESS04;Database=aimachine;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admin_us__3213E83F9AFB7A9D");

            entity.ToTable("admin_users");

            entity.HasIndex(e => e.Username, "UQ__admin_us__F3DBC572B3A3018D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Deleteflag)
                .HasDefaultValue(false)
                .HasColumnName("deleteflag");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__admin_use__creat__70DDC3D8");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.InverseUpdateByNavigation)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__admin_use__updat__71D1E811");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__comments__3213E83FD812ED65");

            entity.ToTable("comments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.JobTitleId).HasColumnName("job_title_id");
            entity.Property(e => e.Message)
                .HasMaxLength(250)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProfileImg)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("profile_img");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("Active") // เอาชื่อ constraint ออกเพื่อความง่าย
                .HasColumnName("status");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.Comments)
                .HasForeignKey(d => d.JobTitleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__comments__job_ti__09A971A2");
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__company___3213E83F280F3B30");

            entity.ToTable("company_profile");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CompannyName)
                .HasMaxLength(100)
                .HasColumnName("companny_name");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FacebookUrl)
                .HasMaxLength(255)
                .HasColumnName("facebook_url");
            entity.Property(e => e.GoogleUrl)
                .HasMaxLength(255)
                .HasColumnName("google_url");
            entity.Property(e => e.YoutubeUrl)
                .HasMaxLength(255)
                .HasColumnName("youtube_url");
            entity.Property(e => e.TiktokUrl)
                .HasMaxLength(255)
                .HasColumnName("tiktok_url");
            entity.Property(e => e.LineId)
                .HasMaxLength(50)
                .HasColumnName("line_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.CompanyProfiles)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__company_p__updat__7D439ABD");
        });

        modelBuilder.Entity<DepartmentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__departme__3213E83F8362EA4B");

            entity.ToTable("department_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CanDelete).HasColumnName("can_delete");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentTitle)
                .HasMaxLength(100)
                .HasColumnName("department_title");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DepartmentTypeCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__departmen__creat__17F790F9");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.DepartmentTypeUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__departmen__updat__18EBB532");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__events__3213E83FDBDDA77B");

            entity.ToTable("events");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.EventDate)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("event_date");
            entity.Property(e => e.Events)
                .HasMaxLength(255)
                .HasColumnName("events");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.Category).WithMany(p => p.Events)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__events__category__778AC167");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EventCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__events__created___787EE5A0");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.EventUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__events__update_b__797309D9");
        });

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__event_ca__3213E83FCE2A4DB1");

            entity.ToTable("event_categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(100)
                .HasColumnName("event_title");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EventCategoryCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__event_cat__creat__7A672E12");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.EventCategoryUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__event_cat__updat__7B5B524B");
        });

        modelBuilder.Entity<EventsImg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__events_i__3213E83FC7D9A366");

            entity.ToTable("events_img");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventsId).HasColumnName("events_id");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.IsCover)
                .HasDefaultValue(false)
                .HasColumnName("isCover");
            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.HasOne(d => d.Events).WithMany(p => p.EventsImgs)
                .HasForeignKey(d => d.EventsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__events_im__event__7C4F7684");
        });

        modelBuilder.Entity<Inbox>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__inbox__3213E83FC9976121");

            entity.ToTable("inbox");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Deleteflag)
                .HasDefaultValue(false)
                .HasColumnName("deleteflag");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Message)
                .HasMaxLength(300)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.TitleId).HasColumnName("title_id");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InboxCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__inbox__created_b__73BA3083");

            entity.HasOne(d => d.Title).WithMany(p => p.Inboxes)
                .HasForeignKey(d => d.TitleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__inbox__title_id__72C60C4A");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.InboxUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__inbox__update_by__74AE54BC");
        });

        modelBuilder.Entity<Intern>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__interns__3213E83F21F4E858");

            entity.ToTable("interns");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime")
                .HasColumnName("date_end");
            entity.Property(e => e.DateOpen)
                .HasColumnType("datetime")
                .HasColumnName("date_open");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.JobTitleId).HasColumnName("job_title_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.TotalPositions).HasColumnName("total_positions");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InternCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__interns__created__7F2BE32F");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.Interns)
                .HasForeignKey(d => d.JobTitleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__interns__job_tit__7E37BEF6");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.InternUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__interns__update___00200768");
        });

        modelBuilder.Entity<InternTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__intern_t__3213E83F31C172B0");

            entity.ToTable("intern_tags");

            entity.HasIndex(e => new { e.InternId, e.StackTagId }, "UQ__intern_t__64970F567C2E714B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InternId).HasColumnName("intern_id");
            entity.Property(e => e.StackTagId).HasColumnName("stack_tag_id");

            entity.HasOne(d => d.Intern).WithMany(p => p.InternTags)
                .HasForeignKey(d => d.InternId)
                .HasConstraintName("FK__intern_ta__inter__01142BA1");

            entity.HasOne(d => d.StackTag).WithMany(p => p.InternTags)
                .HasForeignKey(d => d.StackTagId)
                .HasConstraintName("FK__intern_ta__stack__02084FDA");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__jobs__3213E83F38F9F41C");

            entity.ToTable("jobs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime")
                .HasColumnName("date_end");
            entity.Property(e => e.DateOpen)
                .HasColumnType("datetime")
                .HasColumnName("date_open");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.JobTitleId).HasColumnName("job_title_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.TotalPositions).HasColumnName("total_positions");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__jobs__created_by__03F0984C");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.JobTitleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__jobs__job_title___02FC7413");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.JobUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__jobs__update_by__04E4BC85");
        });

        modelBuilder.Entity<JobTitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__job_titl__3213E83FF3E6B40F");

            entity.ToTable("job_title");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.JobsTitle)
                .HasMaxLength(255)
                .HasColumnName("jobs_title");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobTitleCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__job_title__creat__0B91BA14");

            entity.HasOne(d => d.Department).WithMany(p => p.JobTitles)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__job_title__depar__0A9D95DB");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.JobTitleUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__job_title__updat__0C85DE4D");
        });

        modelBuilder.Entity<JobsTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__jobs_tag__3213E83F9C08E34E");

            entity.ToTable("jobs_tags");

            entity.HasIndex(e => new { e.JobId, e.StackTagId }, "UQ__jobs_tag__C483E5A02A266252").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.StackTagId).HasColumnName("stack_tag_id");

            entity.HasOne(d => d.Job).WithMany(p => p.JobsTags)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__jobs_tags__job_i__07C12930");

            entity.HasOne(d => d.StackTag).WithMany(p => p.JobsTags)
                .HasForeignKey(d => d.StackTagId)
                .HasConstraintName("FK__jobs_tags__stack__08B54D69");
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__partners__3213E83FB5F85A48");

            entity.ToTable("partners");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            // ✅ 1. เพิ่ม Mapping ให้ตรงกับ Database
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");

            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PartnerCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__partners__create__05D8E0BE");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.PartnerUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__partners__update__06CD04F7");

            entity.HasOne(d => d.Department)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId);
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__solution__3213E83FAA9F068F");

            entity.ToTable("solutions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SolutionCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__solutions__creat__0E6E26BF");

            entity.HasOne(d => d.Department).WithMany(p => p.Solutions)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__solutions__depar__0D7A0286");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.SolutionUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__solutions__updat__0F624AF8");
        });

        modelBuilder.Entity<SolutionImg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__solution__3213E83F94273438");

            entity.ToTable("solution_img");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.IsCover)
                .HasDefaultValue(false)
                .HasColumnName("isCover");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.SolutionId).HasColumnName("solution_id");

            entity.HasOne(d => d.Solution).WithMany(p => p.SolutionImgs)
                .HasForeignKey(d => d.SolutionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__solution___solut__10566F31");
        });

        modelBuilder.Entity<TechStack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tech_sta__3213E83FE4627E06");

            entity.ToTable("tech_stacks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Header)
                .HasMaxLength(100)
                .HasColumnName("header");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TechStackCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__tech_stac__creat__114A936A");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.TechStackUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__tech_stac__updat__123EB7A3");
        });

        modelBuilder.Entity<TechStackTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tech_sta__3213E83FC0054143");

            entity.ToTable("tech_stack_tag");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.TechStackTitle)
                .HasMaxLength(100)
                .HasColumnName("tech_stack_title");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TechStackTagCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__tech_stac__creat__160F4887");

            entity.HasOne(d => d.Department).WithMany(p => p.TechStackTags)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tech_stac__depar__151B244E");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.TechStackTagUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__tech_stac__updat__17036CC0");
        });

        modelBuilder.Entity<TechStackTag1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__expertis__3213E83F477F862A");

            entity.ToTable("tech_stack_tags");

            entity.HasIndex(e => new { e.TechId, e.StackTagId }, "UQ__expertis__4A66D27F51C2E907").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StackTagId).HasColumnName("stack_tag_id");
            entity.Property(e => e.TechId).HasColumnName("tech_id");

            entity.HasOne(d => d.StackTag).WithMany(p => p.TechStackTag1s)
                .HasForeignKey(d => d.StackTagId)
                .HasConstraintName("FK__expertise__stack__14270015");

            entity.HasOne(d => d.Tech).WithMany(p => p.TechStackTag1s)
                .HasForeignKey(d => d.TechId)
                .HasConstraintName("FK__expertise__tech___1332DBDC");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__topic__3213E83F33D5FB87");

            entity.ToTable("topic");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.TopicTitle)
                .HasMaxLength(100)
                .HasColumnName("topic_title");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(dateadd(hour,(7),getutcdate()))")
                .HasColumnType("datetime")
                .HasColumnName("update_at");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TopicCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__topic__created_b__75A278F5");

            entity.HasOne(d => d.UpdateByNavigation).WithMany(p => p.TopicUpdateByNavigations)
                .HasForeignKey(d => d.UpdateBy)
                .HasConstraintName("FK__topic__update_by__76969D2E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}