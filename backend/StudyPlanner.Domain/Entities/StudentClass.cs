namespace StudyPlanner.Domain.Entities;

public class StudentClass
{
    public int Id { get; set; }
    public int FacultyId { get; set; }
    public int CohortId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Faculty Faculty { get; set; } = null!;
    public Cohort Cohort { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

