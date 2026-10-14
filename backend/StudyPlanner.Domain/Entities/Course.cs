namespace StudyPlanner.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public int SemesterId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Credits { get; set; }
    public string? Lecturer { get; set; }
    public string? Color { get; set; }
    public Semester Semester { get; set; } = null!;
    public ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

