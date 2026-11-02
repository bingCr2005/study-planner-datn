namespace StudyPlanner.Domain.Entities;

public class Semester
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public string Ten { get; set; } = string.Empty;
    // Lưu trữ chỉ là cờ dữ liệu; API sau này phải quy định quyền sửa và cách lọc học kỳ cũ.
    public bool DaLuuTru { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public User User { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

