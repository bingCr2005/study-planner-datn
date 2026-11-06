using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Domain.Entities;

/// <summary>Lịch chờ phát/thử lại; khác thông báo đã phát trong hộp thư.</summary>
public class LichNhac
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaNhiemVu { get; set; }
    // NULL cho dữ liệu cũ không xác định được offset; tuyệt đối không đoán từ deadline hiện tại.
    public int? NhacTruocPhut { get; set; }
    public string MocNhac { get; set; } = "Deadline";
    public DateTimeOffset ThoiDiemNhac { get; set; }
    public TrangThaiLichNhac TrangThai { get; set; } = TrangThaiLichNhac.Pending;
    public int SoLanThu { get; set; }
    public DateTimeOffset? LanThuTiepTheoLuc { get; set; }
    public DateTimeOffset? NgayPhat { get; set; }
    // ID snapshot UNIQUE để truy ra nguồn chuyển đổi; không FK vòng ngược về thông báo.
    public int? MaThongBaoCu { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public NguoiDung NguoiDung { get; set; } = null!;
    public NhiemVu NhiemVu { get; set; } = null!;
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}
