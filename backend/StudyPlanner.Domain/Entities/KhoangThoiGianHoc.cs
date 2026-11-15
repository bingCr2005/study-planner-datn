namespace StudyPlanner.Domain.Entities;

/// <summary>Khoảng thời gian học liên tục từ lúc bắt đầu/tiếp tục đến lúc tạm dừng/kết thúc.</summary>
public class KhoangThoiGianHoc
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaNhiemVu { get; set; }
    public int? MaPhienTuHoc { get; set; }
    public int? MaPhienTapTrung { get; set; }
    public DateTimeOffset BatDauThucTe { get; set; }
    // NULL nghĩa đang chạy. Thời lượng thực học tính từ các đoạn đã đóng, không lấy giờ dự kiến.
    public DateTimeOffset? KetThucThucTe { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
    public NhiemVu NhiemVu { get; set; } = null!;
    public PhienTuHoc? PhienTuHoc { get; set; }
    public PhienTapTrung? PhienTapTrung { get; set; }
}
