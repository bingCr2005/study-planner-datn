using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class ThongBao
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaNhiemVu { get; set; }
    // Giai đoạn chuyển đổi: chỉ bản Delivered cũ được liên kết; cột lập lịch cũ giữ đến cutover worker.
    public int? MaLichNhac { get; set; }
    public LichNhac? LichNhac { get; set; }
    public LoaiThongBao Loai { get; set; } = LoaiThongBao.DeadlineReminder;
    public string NoiDung { get; set; } = string.Empty;
    // Tiêu đề ngắn của hộp thông báo; nội dung chi tiết vẫn ở NoiDung.
    public string TieuDe { get; set; } = "Nhắc hạn nhiệm vụ";
    public DateTimeOffset ThoiDiemNhac { get; set; }
    public TrangThaiPhatThongBao TrangThaiPhat { get; set; } = TrangThaiPhatThongBao.Pending;
    public DateTimeOffset? NgayPhat { get; set; }
    public DateTimeOffset? NgayDoc { get; set; }
    public int SoLanThu { get; set; }
    public DateTimeOffset? LanThuTiepTheoLuc { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
    public NhiemVu NhiemVu { get; set; } = null!;
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

