using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? CourseId { get; set; }
    // NULL là nhiệm vụ gốc. Chống vòng cây và kiểm tra cùng chủ sở hữu/môn ở Application.
    public int? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType Type { get; set; } = TaskType.Personal;
    // DueDate NULL có thể kế thừa hạn tổ tiên; hạn hiệu lực được tính, không lưu thêm cột.
    public DateTimeOffset? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.NotStarted;
    public int? EstimatedMinutes { get; set; }
    public int Progress { get; set; }
    public int? ReminderHours { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public User User { get; set; } = null!;
    public Course? Course { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> Children { get; set; } = new List<TaskItem>();
    // UNIQUE(TaskId, Type) cùng CHECK Type hiện tại giới hạn tối đa một lời nhắc mỗi nhiệm vụ.
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

