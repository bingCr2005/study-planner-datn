using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
        {
            table.HasCheckConstraint("CK_notifications_attempt_count", "\"AttemptCount\" >= 0");
            table.HasCheckConstraint("CK_notifications_delivery", "(\"DeliveryStatus\" = 'Delivered' AND \"SentAt\" IS NOT NULL) OR (\"DeliveryStatus\" <> 'Delivered' AND \"SentAt\" IS NULL)");
            table.HasCheckConstraint("CK_notifications_read", "\"ReadAt\" IS NULL OR (\"DeliveryStatus\" = 'Delivered' AND \"SentAt\" IS NOT NULL AND \"ReadAt\" >= \"SentAt\")");
            table.HasCheckConstraint("CK_notifications_content_not_blank", "length(btrim(\"Content\")) > 0");
            table.HasCheckConstraint("CK_notifications_type_values", "\"Type\" IN ('DeadlineReminder')");
            table.HasCheckConstraint("CK_notifications_deliverystatus_values", "\"DeliveryStatus\" IN ('Pending', 'Delivered', 'Cancelled', 'Failed')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Content).HasColumnType("text").IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.DeliveryStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.DeliveryStatus).HasDefaultValue(NotificationDeliveryStatus.Pending);
        builder.Property(x => x.AttemptCount).HasDefaultValue(0);
        builder.Property(x => x.RemindAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.SentAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.ReadAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NextAttemptAt).HasColumnType("timestamp with time zone");
        // Type hiện chỉ có DeadlineReminder: kết hợp CHECK và unique tạo tối đa một nhắc/task.
        // Nếu bổ sung Type, cần rà lại cardinality ERD và nghiệp vụ lên lịch.
        builder.HasIndex(x => new { x.TaskId, x.Type }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.SentAt }).HasDatabaseName("IX_notifications_delivered").HasFilter("\"DeliveryStatus\" = 'Delivered'");
        builder.HasIndex(x => new { x.UserId, x.SentAt }, "UnreadInbox").HasDatabaseName("IX_notifications_unread").HasFilter("\"DeliveryStatus\" = 'Delivered' AND \"ReadAt\" IS NULL");
        builder.HasIndex(x => x.RemindAt).HasFilter("\"DeliveryStatus\" = 'Pending'");
        builder.HasIndex(x => x.NextAttemptAt).HasFilter("\"DeliveryStatus\" = 'Failed' AND \"NextAttemptAt\" IS NOT NULL");
        // FK riêng lẻ không bảo đảm Notification.UserId trùng Task.UserId; Application phải kiểm tra.
        builder.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Task).WithMany(x => x.Notifications).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

