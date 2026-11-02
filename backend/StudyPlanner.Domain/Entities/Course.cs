namespace StudyPlanner.Domain.Entities;

public class Course
{
    public int Ma { get; set; }
    public int MaHocKy { get; set; }
    public string? KyHieu { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int? SoTinChi { get; set; }
    public string? GiangVien { get; set; }
    public string? MauSac { get; set; }
    public Semester Semester { get; set; } = null!;
    public ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

