using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class TaskItem
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaMonHoc { get; set; }
    // NULL là nhiệm vụ gốc. Chống vòng cây và kiểm tra cùng chủ sở hữu/môn ở Application.
    public int? MaNhiemVuCha { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public TaskType Loai { get; set; } = TaskType.Personal;
    // HanHoanThanh NULL có thể kế thừa hạn tổ tiên; hạn hiệu lực được tính, không lưu thêm cột.
    public DateTimeOffset? HanHoanThanh { get; set; }
    public TaskPriority MucDoUuTien { get; set; } = TaskPriority.Medium;
    public TaskItemStatus TrangThai { get; set; } = TaskItemStatus.NotStarted;
    public int? ThoiLuongDuKienPhut { get; set; }
    public int TienDo { get; set; }
    public int? NhacTruocGio { get; set; }
    public DateTimeOffset? NgayHoanThanh { get; set; }
    public User User { get; set; } = null!;
    public Course? Course { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> Children { get; set; } = new List<TaskItem>();
    // UNIQUE(MaNhiemVu, Loai) cùng CHECK Loai hiện tại giới hạn tối đa một lời nhắc mỗi nhiệm vụ.
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

