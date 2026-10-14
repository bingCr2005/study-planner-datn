using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("task_items", table =>
        {
            table.HasCheckConstraint("CK_task_items_progress", "\"Progress\" BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_task_items_estimated_minutes", "\"EstimatedMinutes\" IS NULL OR \"EstimatedMinutes\" > 0");
            table.HasCheckConstraint("CK_task_items_reminder_hours", "\"ReminderHours\" IS NULL OR \"ReminderHours\" >= 0");
            table.HasCheckConstraint("CK_task_items_parent", "\"ParentTaskId\" IS NULL OR \"ParentTaskId\" <> \"Id\"");
            table.HasCheckConstraint("CK_task_items_state", "(\"Status\" = 'NotStarted' AND \"Progress\" = 0 AND \"CompletedAt\" IS NULL) OR (\"Status\" = 'InProgress' AND \"Progress\" BETWEEN 1 AND 99 AND \"CompletedAt\" IS NULL) OR (\"Status\" = 'Completed' AND \"Progress\" = 100 AND \"CompletedAt\" IS NOT NULL)");
            table.HasCheckConstraint("CK_task_items_title_not_blank", "length(btrim(\"Title\")) > 0");
            table.HasCheckConstraint("CK_task_items_type_values", "\"Type\" IN ('Assignment', 'Exam', 'Revision', 'Personal')");
            table.HasCheckConstraint("CK_task_items_priority_values", "\"Priority\" IN ('Low', 'Medium', 'High')");
            table.HasCheckConstraint("CK_task_items_status_values", "\"Status\" IN ('NotStarted', 'InProgress', 'Completed')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        // Low = 0 là giá trị CLR mặc định. Sentinel Medium giữ Low do người dùng chọn,
        // tránh EF hiểu Low là "chưa gán" rồi dùng mặc định database Medium.
        builder.Property(x => x.Priority).HasDefaultValue(TaskPriority.Medium).HasSentinel(TaskPriority.Medium);
        builder.Property(x => x.Status).HasDefaultValue(TaskItemStatus.NotStarted);
        builder.Property(x => x.Progress).HasDefaultValue(0);
        builder.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.UserId, x.DueDate });
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasOne(x => x.User).WithMany(x => x.Tasks).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        // Xóa môn giữ nhiệm vụ. Application cần ghi lịch sử bỏ liên kết trước khi xóa môn.
        builder.HasOne(x => x.Course).WithMany(x => x.Tasks).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.SetNull);
        // Cascade chỉ xóa dữ liệu; không tự ghi TaskHistory hoặc tính lại tiến độ tổ tiên.
        builder.HasOne(x => x.ParentTask).WithMany(x => x.Children).HasForeignKey(x => x.ParentTaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

