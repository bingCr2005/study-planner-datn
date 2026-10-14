namespace StudyPlanner.Domain.Entities;

public class ClassSession
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    // Thứ Hai = 1, Chủ nhật = 7; khác cách đánh số của System.DayOfWeek.
    public int DayOfWeek { get; set; }
    // Giờ địa phương lặp theo tuần, diễn giải theo User.TimeZone trong ngày hiệu lực học kỳ.
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Room { get; set; }
    public Course Course { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

