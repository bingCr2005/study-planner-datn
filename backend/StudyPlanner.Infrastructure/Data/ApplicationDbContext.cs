using Microsoft.EntityFrameworkCore;
using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // Legacy tổ chức: ngoài chức năng cốt lõi, vẫn ánh xạ để bảo toàn DB/backfill hồ sơ cũ.
    // Không bỏ DbSet/configuration khi chưa có migration và kế hoạch chuyển dữ liệu tương ứng.
    public DbSet<School> Schools => Set<School>();

    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Cohort> Cohorts => Set<Cohort>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();

    // Dữ liệu cốt lõi dành cho sinh viên.
    public DbSet<User> Users => Set<User>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();
    public DbSet<AvailabilityWindow> AvailabilityWindows => Set<AvailabilityWindow>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<PlanningApplication> PlanningApplications => Set<PlanningApplication>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<FocusInterval> FocusIntervals => Set<FocusInterval>();
    public DbSet<PriorityRule> PriorityRules => Set<PriorityRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nạp mọi IEntityTypeConfiguration trong Infrastructure; chỉ xây dựng model,
        // không tạo bảng hoặc áp dụng migration khi ứng dụng khởi động.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
