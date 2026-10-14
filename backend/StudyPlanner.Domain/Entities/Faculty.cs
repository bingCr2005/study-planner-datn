namespace StudyPlanner.Domain.Entities;

public class Faculty
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    // null! chỉ bỏ cảnh báo nullable; phải tải/gán navigation trước khi đọc School.Name.
    public School School { get; set; } = null!;
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
}
