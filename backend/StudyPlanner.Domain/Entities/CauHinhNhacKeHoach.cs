namespace StudyPlanner.Domain.Entities;

/// <summary>Tùy chọn bật nhắc kế hoạch, tách riêng cấu hình học và thông báo hạn nhiệm vụ.</summary>
public sealed class CauHinhNhacKeHoach
{
    public int MaNguoiDung { get; set; }
    public bool DangBat { get; set; }
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
}
