namespace StudyPlanner.Domain.Entities;

public class School
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // Application gán thời điểm UTC khi tạo/sửa; các thuộc tính không tự cập nhật.
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
}
