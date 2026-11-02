using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Domain.Entities;

/// <summary>Buổi tự học đã lưu; preview gợi ý chưa được chấp nhận không ghi vào bảng này.</summary>
public class StudySession
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaNhiemVu { get; set; }
    public int? MaLanApDung { get; set; }
    // Hai thời điểm tuyệt đối lưu UTC; đây là dự kiến, không dùng để thống kê giờ thực học.
    public DateTimeOffset BatDauDuKien { get; set; }
    public DateTimeOffset KetThucDuKien { get; set; }
    public StudySessionStatus TrangThai { get; set; } = StudySessionStatus.Planned;
    public string? GhiChu { get; set; }
    public User User { get; set; } = null!;
    public TaskItem Task { get; set; } = null!;
    public PlanningApplication? Application { get; set; }
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}
