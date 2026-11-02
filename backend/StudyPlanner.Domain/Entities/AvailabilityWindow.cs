namespace StudyPlanner.Domain.Entities;

/// <summary>Khung sinh viên sẵn sàng học, lặp theo tuần trong khoảng ngày hiệu lực.</summary>
public class AvailabilityWindow
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    // Thứ Hai = 1 ... Chủ nhật = 7. Giờ địa phương theo múi giờ tài khoản, không phải UTC.
    public int ThuTrongTuan { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public bool DangSuDung { get; set; } = true;
    public User User { get; set; } = null!;
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}
