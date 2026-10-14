namespace StudyPlanner.Domain.Entities;

public class Semester
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public User User { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

