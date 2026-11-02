namespace StudyPlanner.Domain.Entities;

public class ClassSession
{
    public int Ma { get; set; }
    public int MaMonHoc { get; set; }
    // Thứ Hai = 1, Chủ nhật = 7; khác cách đánh số của System.DayOfWeek.
    public int ThuTrongTuan { get; set; }
    // Giờ địa phương lặp theo tuần, diễn giải theo User.MuiGio trong ngày hiệu lực học kỳ.
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string? PhongHoc { get; set; }
    // NULL cả cặp: dùng ngày của học kỳ; có cả cặp: lịch chỉ áp dụng trong khoảng này.
    public DateOnly? NgayApDungTu { get; set; }
    public DateOnly? NgayApDungDen { get; set; }
    public Course Course { get; set; } = null!;
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

