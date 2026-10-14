using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public int? ClassId { get; set; }
    public string? Major { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsActive { get; set; } = true;
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public int ReminderHours { get; set; } = 24;
    public StudentClass? Class { get; set; }
    public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

