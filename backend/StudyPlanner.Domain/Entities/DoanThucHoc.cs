namespace StudyPlanner.Domain.Entities;

/// <summary>Một đoạn thực học liên tục: tạm dừng đóng đoạn, tiếp tục tạo đoạn mới.</summary>
public class DoanThucHoc
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaNhiemVu { get; set; }
    public int? MaPhienTuHoc { get; set; }
    public DateTimeOffset BatDauThucTe { get; set; }
    // NULL nghĩa đang chạy. Thời lượng thực học tính từ các đoạn đã đóng, không lấy giờ dự kiến.
    public DateTimeOffset? KetThucThucTe { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
    public NhiemVu NhiemVu { get; set; } = null!;
    public PhienTuHoc? PhienTuHoc { get; set; }
}
