using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {

        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(x => x.MaNhiemVu).HasColumnName("ma_nhiem_vu");
        builder.Property(x => x.Loai).HasColumnName("loai");
        builder.Property(x => x.NoiDung).HasColumnName("noi_dung");
        builder.Property(x => x.ThoiDiemNhac).HasColumnName("thoi_diem_nhac");
        builder.Property(x => x.TrangThaiPhat).HasColumnName("trang_thai_phat");
        builder.Property(x => x.NgayPhat).HasColumnName("ngay_phat");
        builder.Property(x => x.NgayDoc).HasColumnName("ngay_doc");
        builder.Property(x => x.SoLanThu).HasColumnName("so_lan_thu");
        builder.Property(x => x.LanThuTiepTheoLuc).HasColumnName("lan_thu_tiep_theo_luc");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("notifications", table =>
        {
            table.HasCheckConstraint("CK_notifications_attempt_count", "\"so_lan_thu\" >= 0");
            table.HasCheckConstraint("CK_notifications_delivery", "(\"trang_thai_phat\" = 'Delivered' AND \"ngay_phat\" IS NOT NULL) OR (\"trang_thai_phat\" <> 'Delivered' AND \"ngay_phat\" IS NULL)");
            table.HasCheckConstraint("CK_notifications_read", "\"ngay_doc\" IS NULL OR (\"trang_thai_phat\" = 'Delivered' AND \"ngay_phat\" IS NOT NULL AND \"ngay_doc\" >= \"ngay_phat\")");
            table.HasCheckConstraint("CK_notifications_content_not_blank", "length(btrim(\"noi_dung\")) > 0");
            table.HasCheckConstraint("CK_notifications_type_values", "\"loai\" IN ('DeadlineReminder')");
            table.HasCheckConstraint("CK_notifications_deliverystatus_values", "\"trang_thai_phat\" IN ('Pending', 'Delivered', 'Cancelled', 'Failed')");
        });
        builder.HasKey(x => x.Ma).HasName("PK_notifications");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.NoiDung).HasColumnType("text").IsRequired();
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.TrangThaiPhat).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        builder.Property(x => x.TrangThaiPhat).HasDefaultValue(NotificationDeliveryStatus.Pending);
        builder.Property(x => x.SoLanThu).HasDefaultValue(0);
        builder.Property(x => x.ThoiDiemNhac).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayPhat).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayDoc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.LanThuTiepTheoLuc).HasColumnType("timestamp with time zone");
        // Type hiện chỉ có DeadlineReminder: kết hợp CHECK và unique tạo tối đa một nhắc/task.
        // Nếu bổ sung Type, cần rà lại cardinality ERD và nghiệp vụ lên lịch.
        builder.HasIndex(x => new { x.MaNhiemVu, x.Loai }).HasDatabaseName("IX_notifications_TaskId_Type").IsUnique();
        builder.HasIndex(x => new { x.MaNguoiDung, x.NgayPhat }).HasDatabaseName("IX_notifications_delivered").HasFilter("\"trang_thai_phat\" = 'Delivered'");
        builder.HasIndex(x => new { x.MaNguoiDung, x.NgayPhat }, "UnreadInbox").HasDatabaseName("IX_notifications_unread").HasFilter("\"trang_thai_phat\" = 'Delivered' AND \"ngay_doc\" IS NULL");
        builder.HasIndex(x => x.ThoiDiemNhac).HasDatabaseName("IX_notifications_RemindAt").HasFilter("\"trang_thai_phat\" = 'Pending'");
        builder.HasIndex(x => x.LanThuTiepTheoLuc).HasDatabaseName("IX_notifications_NextAttemptAt").HasFilter("\"trang_thai_phat\" = 'Failed' AND \"lan_thu_tiep_theo_luc\" IS NOT NULL");
        // FK riêng lẻ không bảo đảm Notification.MaNguoiDung trùng Task.MaNguoiDung; Application phải kiểm tra.
        builder.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.MaNguoiDung).HasConstraintName("FK_notifications_users_UserId").OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Task).WithMany(x => x.Notifications).HasForeignKey(x => x.MaNhiemVu).HasConstraintName("FK_notifications_task_items_TaskId").OnDelete(DeleteBehavior.Cascade);
    }
}

