namespace StudyPlanner.Domain.Entities;

/// <summary>Lần áp dụng lịch thành công: lưu key và dấu kiểm để request gửi lại không tạo thêm phiên.</summary>
public class PlanningApplication
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public string KhoaYeuCau { get; set; } = string.Empty;
    // SHA-256 dạng hex của request chuẩn hóa, không chứa token/mật khẩu.
    public string DauKiemNoiDung { get; set; } = string.Empty;
    public long PhienBanNguon { get; set; }
    public int PhienBanQuyTac { get; set; }
    public DateTimeOffset NgayTao { get; set; }
    public User User { get; set; } = null!;
    // UNIQUE key chỉ chống lặp request. Dịch vụ apply vẫn phải khóa User và kiểm tra trùng giờ.
}
