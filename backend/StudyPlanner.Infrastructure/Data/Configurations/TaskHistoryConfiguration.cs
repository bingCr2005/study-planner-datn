using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.ToTable("task_histories", table =>
        {
            table.HasCheckConstraint("CK_task_histories_tasktitle_not_blank", "length(btrim(\"TaskTitle\")) > 0");
            table.HasCheckConstraint("CK_task_histories_action_values", "\"Action\" IN ('Created', 'Updated', 'Completed', 'Reopened', 'Deleted')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.TaskTitle).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.OldData).HasColumnType("jsonb");
        builder.Property(x => x.NewData).HasColumnType("jsonb");
        builder.Property(x => x.ChangedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.UserId, x.ChangedAt });
        builder.HasIndex(x => new { x.TaskId, x.ChangedAt });
        // Xóa tài khoản giữ sự kiện nhưng bỏ liên kết danh tính; TaskId cố ý không có FK.
        builder.HasOne(x => x.User).WithMany(x => x.TaskHistories).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}

