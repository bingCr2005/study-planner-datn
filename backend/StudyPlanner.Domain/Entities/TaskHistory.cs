using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class TaskHistory
{
    public int Id { get; set; }
    // Mã chụp lại: không có navigation/FK tới TaskItem để lịch sử còn sau khi xóa nhiệm vụ.
    public int TaskId { get; set; }
    public int? UserId { get; set; }
    public TaskHistoryAction Action { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    // Chuỗi JSON ánh xạ jsonb; chỉ serialize trường nhiệm vụ cho phép, không lưu bí mật.
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public User? User { get; set; }
}

