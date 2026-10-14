using Microsoft.EntityFrameworkCore;
using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<School> Schools => Set<School>();

    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Cohort> Cohorts => Set<Cohort>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nạp mọi IEntityTypeConfiguration trong Infrastructure; chỉ xây dựng model,
        // không tạo bảng hoặc áp dụng migration khi ứng dụng khởi động.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
