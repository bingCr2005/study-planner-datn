using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TaskId { get; set; }
    public NotificationType Type { get; set; } = NotificationType.DeadlineReminder;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset RemindAt { get; set; }
    public NotificationDeliveryStatus DeliveryStatus { get; set; } = NotificationDeliveryStatus.Pending;
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public User User { get; set; } = null!;
    public TaskItem Task { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

