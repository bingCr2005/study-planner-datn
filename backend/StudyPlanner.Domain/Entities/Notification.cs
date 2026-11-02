using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class Notification
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaNhiemVu { get; set; }
    public NotificationType Loai { get; set; } = NotificationType.DeadlineReminder;
    public string NoiDung { get; set; } = string.Empty;
    public DateTimeOffset ThoiDiemNhac { get; set; }
    public NotificationDeliveryStatus TrangThaiPhat { get; set; } = NotificationDeliveryStatus.Pending;
    public DateTimeOffset? NgayPhat { get; set; }
    public DateTimeOffset? NgayDoc { get; set; }
    public int SoLanThu { get; set; }
    public DateTimeOffset? LanThuTiepTheoLuc { get; set; }
    public User User { get; set; } = null!;
    public TaskItem Task { get; set; } = null!;
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

