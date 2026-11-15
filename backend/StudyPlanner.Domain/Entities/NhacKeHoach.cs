namespace StudyPlanner.Domain.Entities;

/// <summary>Nhắc kế hoạch được lưu để xem trong ứng dụng, không cần tạo nhiệm vụ giả làm nguồn nhắc.</summary>
public sealed class NhacKeHoach
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public string KhoaChongTrung { get; set; } = "";
    public string Loai { get; set; } = "WeeklyReview";
    public string TieuDe { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public string DuongDan { get; set; } = "/tasks?tab=planning";
    public DateTimeOffset NgayPhat { get; set; }
    public DateTimeOffset? NgayDoc { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
}
