using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aimachine.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    deleteflag = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_admin_users_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_admin_users_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_profile",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    companny_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    google_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    facebook_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    youtube_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tiktok_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    line_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_profile", x => x.id);
                    table.ForeignKey(
                        name: "FK_company_profile_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "department_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    can_delete = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_department_types_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_department_types_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "event_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_categories_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_event_categories_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tech_stacks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    header = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tech_stacks", x => x.id);
                    table.ForeignKey(
                        name: "FK_tech_stacks_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tech_stacks_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "topic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    topic_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_topic_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_title",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    jobs_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_title", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_title_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_job_title_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_job_title_department_types_department_id",
                        column: x => x.department_id,
                        principalTable: "department_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    image = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.id);
                    table.ForeignKey(
                        name: "FK_partners_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_partners_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_partners_department_types_department_id",
                        column: x => x.department_id,
                        principalTable: "department_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "solutions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_solutions_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_solutions_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_solutions_department_types_department_id",
                        column: x => x.department_id,
                        principalTable: "department_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tech_stack_tag",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    tech_stack_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tech_stack_tag", x => x.id);
                    table.ForeignKey(
                        name: "FK_tech_stack_tag_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tech_stack_tag_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tech_stack_tag_department_types_department_id",
                        column: x => x.department_id,
                        principalTable: "department_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    events = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    event_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_events_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_events_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_events_event_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "event_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inbox",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleteflag = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox", x => x.id);
                    table.ForeignKey(
                        name: "FK_inbox_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_inbox_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_inbox_topic_title_id",
                        column: x => x.title_id,
                        principalTable: "topic",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    profile_img = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    job_title_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    message = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_comments_job_title_job_title_id",
                        column: x => x.job_title_id,
                        principalTable: "job_title",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "interns",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_title_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    total_positions = table.Column<int>(type: "integer", nullable: true),
                    date_open = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    date_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interns", x => x.id);
                    table.ForeignKey(
                        name: "FK_interns_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_interns_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_interns_job_title_job_title_id",
                        column: x => x.job_title_id,
                        principalTable: "job_title",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_title_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    total_positions = table.Column<int>(type: "integer", nullable: true),
                    date_open = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    date_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    update_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_jobs_admin_users_created_by",
                        column: x => x.created_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_jobs_admin_users_update_by",
                        column: x => x.update_by,
                        principalTable: "admin_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_jobs_job_title_job_title_id",
                        column: x => x.job_title_id,
                        principalTable: "job_title",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "solution_img",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solution_id = table.Column<int>(type: "integer", nullable: false),
                    image = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isCover = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    order_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solution_img", x => x.id);
                    table.ForeignKey(
                        name: "FK_solution_img_solutions_solution_id",
                        column: x => x.solution_id,
                        principalTable: "solutions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tech_stack_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tech_id = table.Column<int>(type: "integer", nullable: true),
                    stack_tag_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tech_stack_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_tech_stack_tags_tech_stack_tag_stack_tag_id",
                        column: x => x.stack_tag_id,
                        principalTable: "tech_stack_tag",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tech_stack_tags_tech_stacks_tech_id",
                        column: x => x.tech_id,
                        principalTable: "tech_stacks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "events_img",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    events_id = table.Column<int>(type: "integer", nullable: false),
                    image = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isCover = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    order_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events_img", x => x.id);
                    table.ForeignKey(
                        name: "FK_events_img_events_events_id",
                        column: x => x.events_id,
                        principalTable: "events",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "intern_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    intern_id = table.Column<int>(type: "integer", nullable: true),
                    stack_tag_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intern_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_intern_tags_interns_intern_id",
                        column: x => x.intern_id,
                        principalTable: "interns",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_intern_tags_tech_stack_tag_stack_tag_id",
                        column: x => x.stack_tag_id,
                        principalTable: "tech_stack_tag",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "jobs_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_id = table.Column<int>(type: "integer", nullable: true),
                    stack_tag_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_jobs_tags_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_jobs_tags_tech_stack_tag_stack_tag_id",
                        column: x => x.stack_tag_id,
                        principalTable: "tech_stack_tag",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_created_by",
                table: "admin_users",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_update_by",
                table: "admin_users",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_username",
                table: "admin_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comments_job_title_id",
                table: "comments",
                column: "job_title_id");

            migrationBuilder.CreateIndex(
                name: "IX_company_profile_update_by",
                table: "company_profile",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_department_types_created_by",
                table: "department_types",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_department_types_update_by",
                table: "department_types",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_event_categories_created_by",
                table: "event_categories",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_event_categories_update_by",
                table: "event_categories",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_events_category_id",
                table: "events",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_created_by",
                table: "events",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_events_update_by",
                table: "events",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_events_img_events_id",
                table: "events_img",
                column: "events_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_created_by",
                table: "inbox",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_title_id",
                table: "inbox",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_update_by",
                table: "inbox",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_intern_tags_intern_id_stack_tag_id",
                table: "intern_tags",
                columns: new[] { "intern_id", "stack_tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_intern_tags_stack_tag_id",
                table: "intern_tags",
                column: "stack_tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_interns_created_by",
                table: "interns",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_interns_job_title_id",
                table: "interns",
                column: "job_title_id");

            migrationBuilder.CreateIndex(
                name: "IX_interns_update_by",
                table: "interns",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_job_title_created_by",
                table: "job_title",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_job_title_department_id",
                table: "job_title",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_title_update_by",
                table: "job_title",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_created_by",
                table: "jobs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_job_title_id",
                table: "jobs",
                column: "job_title_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_update_by",
                table: "jobs",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_tags_job_id_stack_tag_id",
                table: "jobs_tags",
                columns: new[] { "job_id", "stack_tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_jobs_tags_stack_tag_id",
                table: "jobs_tags",
                column: "stack_tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_partners_created_by",
                table: "partners",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_partners_department_id",
                table: "partners",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_partners_update_by",
                table: "partners",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_solution_img_solution_id",
                table: "solution_img",
                column: "solution_id");

            migrationBuilder.CreateIndex(
                name: "IX_solutions_created_by",
                table: "solutions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_solutions_department_id",
                table: "solutions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_solutions_update_by",
                table: "solutions",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_tag_created_by",
                table: "tech_stack_tag",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_tag_department_id",
                table: "tech_stack_tag",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_tag_update_by",
                table: "tech_stack_tag",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_tags_stack_tag_id",
                table: "tech_stack_tags",
                column: "stack_tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stack_tags_tech_id_stack_tag_id",
                table: "tech_stack_tags",
                columns: new[] { "tech_id", "stack_tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tech_stacks_created_by",
                table: "tech_stacks",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tech_stacks_update_by",
                table: "tech_stacks",
                column: "update_by");

            migrationBuilder.CreateIndex(
                name: "IX_topic_created_by",
                table: "topic",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_topic_update_by",
                table: "topic",
                column: "update_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "company_profile");

            migrationBuilder.DropTable(
                name: "events_img");

            migrationBuilder.DropTable(
                name: "inbox");

            migrationBuilder.DropTable(
                name: "intern_tags");

            migrationBuilder.DropTable(
                name: "jobs_tags");

            migrationBuilder.DropTable(
                name: "partners");

            migrationBuilder.DropTable(
                name: "solution_img");

            migrationBuilder.DropTable(
                name: "tech_stack_tags");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "topic");

            migrationBuilder.DropTable(
                name: "interns");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "solutions");

            migrationBuilder.DropTable(
                name: "tech_stack_tag");

            migrationBuilder.DropTable(
                name: "tech_stacks");

            migrationBuilder.DropTable(
                name: "event_categories");

            migrationBuilder.DropTable(
                name: "job_title");

            migrationBuilder.DropTable(
                name: "department_types");

            migrationBuilder.DropTable(
                name: "admin_users");
        }
    }
}
