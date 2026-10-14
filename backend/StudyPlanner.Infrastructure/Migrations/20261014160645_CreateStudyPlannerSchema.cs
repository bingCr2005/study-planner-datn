using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateStudyPlannerSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cohorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    EndYear = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cohorts", x => x.Id);
                    table.CheckConstraint("CK_cohorts_name_not_blank", "length(btrim(\"Name\")) > 0");
                    table.CheckConstraint("CK_cohorts_years", "\"EndYear\" >= \"StartYear\"");
                });

            migrationBuilder.CreateTable(
                name: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.Id);
                    table.CheckConstraint("CK_schools_code_not_blank", "length(btrim(\"Code\")) > 0");
                    table.CheckConstraint("CK_schools_name_not_blank", "length(btrim(\"Name\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faculties", x => x.Id);
                    table.CheckConstraint("CK_faculties_code_not_blank", "length(btrim(\"Code\")) > 0");
                    table.CheckConstraint("CK_faculties_name_not_blank", "length(btrim(\"Name\")) > 0");
                    table.ForeignKey(
                        name: "FK_faculties_schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FacultyId = table.Column<int>(type: "integer", nullable: false),
                    CohortId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_classes", x => x.Id);
                    table.CheckConstraint("CK_student_classes_name_not_blank", "length(btrim(\"Name\")) > 0");
                    table.ForeignKey(
                        name: "FK_student_classes_cohorts_CohortId",
                        column: x => x.CohortId,
                        principalTable: "cohorts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_classes_faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClassId = table.Column<int>(type: "integer", nullable: true),
                    Major = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Student"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Asia/Ho_Chi_Minh"),
                    ReminderHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.CheckConstraint("CK_users_email_not_blank", "length(btrim(\"Email\")) > 0");
                    table.CheckConstraint("CK_users_fullname_not_blank", "length(btrim(\"FullName\")) > 0");
                    table.CheckConstraint("CK_users_normalizedemail_not_blank", "length(btrim(\"NormalizedEmail\")) > 0");
                    table.CheckConstraint("CK_users_passwordhash_not_blank", "length(btrim(\"PasswordHash\")) > 0");
                    table.CheckConstraint("CK_users_reminder_hours", "\"ReminderHours\" >= 0");
                    table.CheckConstraint("CK_users_role_values", "\"Role\" IN ('Student', 'Admin')");
                    table.CheckConstraint("CK_users_student_code", "\"StudentCode\" IS NULL OR length(btrim(\"StudentCode\")) > 0");
                    table.CheckConstraint("CK_users_timezone_not_blank", "length(btrim(\"TimeZone\")) > 0");
                    table.ForeignKey(
                        name: "FK_users_student_classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "student_classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semesters", x => x.Id);
                    table.CheckConstraint("CK_semesters_dates", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_semesters_name_not_blank", "length(btrim(\"Name\")) > 0");
                    table.ForeignKey(
                        name: "FK_semesters_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TaskTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OldData = table.Column<string>(type: "jsonb", nullable: true),
                    NewData = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_histories", x => x.Id);
                    table.CheckConstraint("CK_task_histories_action_values", "\"Action\" IN ('Created', 'Updated', 'Completed', 'Reopened', 'Deleted')");
                    table.CheckConstraint("CK_task_histories_tasktitle_not_blank", "length(btrim(\"TaskTitle\")) > 0");
                    table.ForeignKey(
                        name: "FK_task_histories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SemesterId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Credits = table.Column<int>(type: "integer", nullable: true),
                    Lecturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.Id);
                    table.CheckConstraint("CK_courses_color", "\"Color\" IS NULL OR \"Color\" ~ '^#[0-9A-Fa-f]{6}$'");
                    table.CheckConstraint("CK_courses_credits", "\"Credits\" IS NULL OR \"Credits\" > 0");
                    table.CheckConstraint("CK_courses_name_not_blank", "length(btrim(\"Name\")) > 0");
                    table.ForeignKey(
                        name: "FK_courses_semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "class_sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Room = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_sessions", x => x.Id);
                    table.CheckConstraint("CK_class_sessions_times", "\"EndTime\" > \"StartTime\"");
                    table.CheckConstraint("CK_class_sessions_weekday", "\"DayOfWeek\" BETWEEN 1 AND 7");
                    table.ForeignKey(
                        name: "FK_class_sessions_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: true),
                    ParentTaskId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Medium"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "NotStarted"),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: true),
                    Progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReminderHours = table.Column<int>(type: "integer", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_items", x => x.Id);
                    table.CheckConstraint("CK_task_items_estimated_minutes", "\"EstimatedMinutes\" IS NULL OR \"EstimatedMinutes\" > 0");
                    table.CheckConstraint("CK_task_items_parent", "\"ParentTaskId\" IS NULL OR \"ParentTaskId\" <> \"Id\"");
                    table.CheckConstraint("CK_task_items_priority_values", "\"Priority\" IN ('Low', 'Medium', 'High')");
                    table.CheckConstraint("CK_task_items_progress", "\"Progress\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_task_items_reminder_hours", "\"ReminderHours\" IS NULL OR \"ReminderHours\" >= 0");
                    table.CheckConstraint("CK_task_items_state", "(\"Status\" = 'NotStarted' AND \"Progress\" = 0 AND \"CompletedAt\" IS NULL) OR (\"Status\" = 'InProgress' AND \"Progress\" BETWEEN 1 AND 99 AND \"CompletedAt\" IS NULL) OR (\"Status\" = 'Completed' AND \"Progress\" = 100 AND \"CompletedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_task_items_status_values", "\"Status\" IN ('NotStarted', 'InProgress', 'Completed')");
                    table.CheckConstraint("CK_task_items_title_not_blank", "length(btrim(\"Title\")) > 0");
                    table.CheckConstraint("CK_task_items_type_values", "\"Type\" IN ('Assignment', 'Exam', 'Revision', 'Personal')");
                    table.ForeignKey(
                        name: "FK_task_items_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_task_items_task_items_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "task_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_items_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    RemindAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.CheckConstraint("CK_notifications_attempt_count", "\"AttemptCount\" >= 0");
                    table.CheckConstraint("CK_notifications_content_not_blank", "length(btrim(\"Content\")) > 0");
                    table.CheckConstraint("CK_notifications_delivery", "(\"DeliveryStatus\" = 'Delivered' AND \"SentAt\" IS NOT NULL) OR (\"DeliveryStatus\" <> 'Delivered' AND \"SentAt\" IS NULL)");
                    table.CheckConstraint("CK_notifications_deliverystatus_values", "\"DeliveryStatus\" IN ('Pending', 'Delivered', 'Cancelled', 'Failed')");
                    table.CheckConstraint("CK_notifications_read", "\"ReadAt\" IS NULL OR (\"DeliveryStatus\" = 'Delivered' AND \"SentAt\" IS NOT NULL AND \"ReadAt\" >= \"SentAt\")");
                    table.CheckConstraint("CK_notifications_type_values", "\"Type\" IN ('DeadlineReminder')");
                    table.ForeignKey(
                        name: "FK_notifications_task_items_TaskId",
                        column: x => x.TaskId,
                        principalTable: "task_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_sessions_CourseId_DayOfWeek_StartTime",
                table: "class_sessions",
                columns: new[] { "CourseId", "DayOfWeek", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_cohorts_Name",
                table: "cohorts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_SemesterId",
                table: "courses",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_faculties_SchoolId_Code",
                table: "faculties",
                columns: new[] { "SchoolId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_delivered",
                table: "notifications",
                columns: new[] { "UserId", "SentAt" },
                filter: "\"DeliveryStatus\" = 'Delivered'");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_NextAttemptAt",
                table: "notifications",
                column: "NextAttemptAt",
                filter: "\"DeliveryStatus\" = 'Failed' AND \"NextAttemptAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RemindAt",
                table: "notifications",
                column: "RemindAt",
                filter: "\"DeliveryStatus\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_TaskId_Type",
                table: "notifications",
                columns: new[] { "TaskId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_unread",
                table: "notifications",
                columns: new[] { "UserId", "SentAt" },
                filter: "\"DeliveryStatus\" = 'Delivered' AND \"ReadAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_schools_Code",
                table: "schools",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_semesters_UserId_StartDate",
                table: "semesters",
                columns: new[] { "UserId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_student_classes_CohortId",
                table: "student_classes",
                column: "CohortId");

            migrationBuilder.CreateIndex(
                name: "IX_student_classes_FacultyId_CohortId_Name",
                table: "student_classes",
                columns: new[] { "FacultyId", "CohortId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_histories_TaskId_ChangedAt",
                table: "task_histories",
                columns: new[] { "TaskId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_task_histories_UserId_ChangedAt",
                table: "task_histories",
                columns: new[] { "UserId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_task_items_CourseId",
                table: "task_items",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_task_items_ParentTaskId",
                table: "task_items",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_task_items_UserId_DueDate",
                table: "task_items",
                columns: new[] { "UserId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_task_items_UserId_Status",
                table: "task_items",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_users_ClassId",
                table: "users",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_users_NormalizedEmail",
                table: "users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_StudentCode",
                table: "users",
                column: "StudentCode",
                unique: true,
                filter: "\"StudentCode\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_sessions");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "task_histories");

            migrationBuilder.DropTable(
                name: "task_items");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "semesters");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "student_classes");

            migrationBuilder.DropTable(
                name: "cohorts");

            migrationBuilder.DropTable(
                name: "faculties");

            migrationBuilder.DropTable(
                name: "schools");
        }
    }
}
