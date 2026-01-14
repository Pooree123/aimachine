CREATE TABLE [admin_users] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [username] nvarchar(50) UNIQUE NOT NULL,
  [full_name] nvarchar(100),
  [password_hash] nvarchar(255) NOT NULL,
  [status] nvarchar(255) NOT NULL CHECK ([status] IN ('Active', 'inActive')),
  [deleteflag] bit DEFAULT (0),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [inbox] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [title_id] int NOT NULL,
  [name] nvarchar(100),
  [message] nvarchar(300),
  [phone] varchar(10),
  [email] nvachar(100),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [topic] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [topic_title] nvarchar(100),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [events] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [category_id] int NOT NULL,
  [events] nvarchar(255),
  [location] nvarchar(255),
  [event_date] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [description] nvarchar(255),
  [status] nvarchar(255) NOT NULL CHECK ([status] IN ('Active', 'inActive')),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [event_categories] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [event_title] nvarchar(100),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [events_img] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [events_id] int NOT NULL,
  [image] nvarchar(255),
  [isCover] bit DEFAULT (0),
  [order_id] int
)
GO

CREATE TABLE [company_profile] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [companny_name] nvarchar(100),
  [description] nvarchar(255),
  [email] nvarchar(100),
  [phone] varchar(10),
  [address] nvarchar(255),
  [google_url] nvarchar(255),
  [facebook_url] nvarchar(255),
  [line_id] nvarchar(50),
  [update_by] int,
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [interns] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [job_title_id] int NOT NULL,
  [description] nvarchar(255),
  [total_positions] int,
  [date_open] DATETIME,
  [date_end] DATETIME,
  [status] nvarchar(255) NOT NULL CHECK ([status] IN ('Active', 'inActive')),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [intern_tags] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [intern_id] int,
  [stack_tag_id] int,
  PRIMARY KEY ([intern_id], [stack_tag_id])
)
GO

CREATE TABLE [jobs] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [job_title_id] int NOT NULL,
  [description] nvarchar(255),
  [total_positions] int,
  [date_open] DATETIME,
  [date_end] DATETIME,
  [status] nvarchar(255) NOT NULL CHECK ([status] IN ('Active', 'inActive')),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [partners] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [image] varchar(255),
  [name] varchar(255),
  [status] nvarchar(255) NOT NULL CHECK ([status] IN ('Active', 'inActive')),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [jobs_tags] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [job_id] int,
  [stack_tag_id] int,
  PRIMARY KEY ([job_id], [stack_tag_id])
)
GO

CREATE TABLE [comments] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [profile_img] varchar(255),
  [job_title_id] int NOT NULL,
  [name] nvarchar(100),
  [message] nvarchar(250),
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [job_title] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [department_id] int NOT NULL,
  [jobs_title] nvarchar(255),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [solutions] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [department_id] int NOT NULL,
  [name] nvarchar(100),
  [description] nvarchar(255),
  [status] nvarchar(255) NOT NULL CHECK ([status] IN ('Active', 'inActive')),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [solution_img] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [solution_id] int NOT NULL,
  [image] varchar(255),
  [isCover] bit DEFAULT (0),
  [order_id] int
)
GO

CREATE TABLE [tech_stacks] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [header] nvarchar(100),
  [description] nvarchar(255),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [expertise_stat_tags] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [tech_id] int,
  [stack_tag_id] int,
  PRIMARY KEY ([tech_id], [stack_tag_id])
)
GO

CREATE TABLE [tech_stack_tag] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [department_id] int NOT NULL,
  [expertise_title] nvarchar(100),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

CREATE TABLE [department_types] (
  [id] int PRIMARY KEY IDENTITY(1, 1),
  [department_title] nvarchar(100),
  [created_by] int,
  [update_by] int,
  [created_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE())),
  [update_at] DATETIME DEFAULT (DATEADD(HOUR, 7, GETUTCDATE()))
)
GO

ALTER TABLE [admin_users] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [admin_users] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [inbox] ADD FOREIGN KEY ([title_id]) REFERENCES [topic] ([id])
GO

ALTER TABLE [inbox] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [inbox] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [topic] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [topic] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [events] ADD FOREIGN KEY ([category_id]) REFERENCES [event_categories] ([id])
GO

ALTER TABLE [events] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [events] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [event_categories] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [event_categories] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [events_img] ADD FOREIGN KEY ([events_id]) REFERENCES [events] ([id])
GO

ALTER TABLE [company_profile] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [interns] ADD FOREIGN KEY ([job_title_id]) REFERENCES [job_title] ([id])
GO

ALTER TABLE [interns] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [interns] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [intern_tags] ADD FOREIGN KEY ([intern_id]) REFERENCES [interns] ([id])
GO

ALTER TABLE [intern_tags] ADD FOREIGN KEY ([stack_tag_id]) REFERENCES [tech_stack_tag] ([id])
GO

ALTER TABLE [jobs] ADD FOREIGN KEY ([job_title_id]) REFERENCES [job_title] ([id])
GO

ALTER TABLE [jobs] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [jobs] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [partners] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [partners] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [jobs_tags] ADD FOREIGN KEY ([job_id]) REFERENCES [jobs] ([id])
GO

ALTER TABLE [jobs_tags] ADD FOREIGN KEY ([stack_tag_id]) REFERENCES [tech_stack_tag] ([id])
GO

ALTER TABLE [comments] ADD FOREIGN KEY ([job_title_id]) REFERENCES [job_title] ([id])
GO

ALTER TABLE [job_title] ADD FOREIGN KEY ([department_id]) REFERENCES [department_types] ([id])
GO

ALTER TABLE [job_title] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [job_title] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [solutions] ADD FOREIGN KEY ([department_id]) REFERENCES [department_types] ([id])
GO

ALTER TABLE [solutions] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [solutions] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [solution_img] ADD FOREIGN KEY ([solution_id]) REFERENCES [solutions] ([id])
GO

ALTER TABLE [tech_stacks] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [tech_stacks] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [expertise_stat_tags] ADD FOREIGN KEY ([tech_id]) REFERENCES [tech_stacks] ([id])
GO

ALTER TABLE [expertise_stat_tags] ADD FOREIGN KEY ([stack_tag_id]) REFERENCES [tech_stack_tag] ([id])
GO

ALTER TABLE [tech_stack_tag] ADD FOREIGN KEY ([department_id]) REFERENCES [department_types] ([id])
GO

ALTER TABLE [tech_stack_tag] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [tech_stack_tag] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [department_types] ADD FOREIGN KEY ([created_by]) REFERENCES [admin_users] ([id])
GO

ALTER TABLE [department_types] ADD FOREIGN KEY ([update_by]) REFERENCES [admin_users] ([id])
GO
