namespace StudyPlanner.Domain.Entities;

public class Cohort
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

