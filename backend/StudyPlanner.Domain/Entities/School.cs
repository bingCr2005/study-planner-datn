namespace StudyPlanner.Domain.Entities;

public class School
{
    public int Ma { get; set; }
    public string KyHieu { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    // Application gán thời điểm UTC khi tạo/sửa; các thuộc tính không tự cập nhật.
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
    public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
}
