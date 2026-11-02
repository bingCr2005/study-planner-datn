using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class User
{
    public int Ma { get; set; }
    public string Email { get; set; } = string.Empty;
    public string EmailChuanHoa { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string MatKhauBam { get; set; } = string.Empty;
    public string? MaSoSinhVien { get; set; }
    public int? MaLop { get; set; }
    public string? NganhHoc { get; set; }
    public UserRole VaiTro { get; set; } = UserRole.Student;
    public bool DangHoatDong { get; set; } = true;
    public string MuiGio { get; set; } = "Asia/Ho_Chi_Minh";
    public int NhacTruocGio { get; set; } = 24;
    public StudentClass? Class { get; set; }
    public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

