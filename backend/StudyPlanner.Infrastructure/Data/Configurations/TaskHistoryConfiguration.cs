using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {

        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaNhiemVu).HasColumnName("ma_nhiem_vu");
        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(x => x.HanhDong).HasColumnName("hanh_dong");
        builder.Property(x => x.TieuDeNhiemVu).HasColumnName("tieu_de_nhiem_vu");
        builder.Property(x => x.DuLieuTruoc).HasColumnName("du_lieu_truoc");
        builder.Property(x => x.DuLieuSau).HasColumnName("du_lieu_sau");
        builder.Property(x => x.NgayThayDoi).HasColumnName("ngay_thay_doi");
        builder.ToTable("task_histories", table =>
        {
            table.HasCheckConstraint("CK_task_histories_tasktitle_not_blank", "length(btrim(\"tieu_de_nhiem_vu\")) > 0");
            table.HasCheckConstraint("CK_task_histories_action_values", "\"hanh_dong\" IN ('Created', 'Updated', 'Completed', 'Reopened', 'Deleted')");
        });
        builder.HasKey(x => x.Ma).HasName("PK_task_histories");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.TieuDeNhiemVu).HasMaxLength(200).IsRequired();
        builder.Property(x => x.HanhDong).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.DuLieuTruoc).HasColumnType("jsonb");
        builder.Property(x => x.DuLieuSau).HasColumnType("jsonb");
        builder.Property(x => x.NgayThayDoi).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.MaNguoiDung, x.NgayThayDoi }).HasDatabaseName("IX_task_histories_UserId_ChangedAt");
        builder.HasIndex(x => new { x.MaNhiemVu, x.NgayThayDoi }).HasDatabaseName("IX_task_histories_TaskId_ChangedAt");
        // Xóa tài khoản giữ sự kiện nhưng bỏ liên kết danh tính; TaskId cố ý không có FK.
        builder.HasOne(x => x.User).WithMany(x => x.TaskHistories).HasForeignKey(x => x.MaNguoiDung).HasConstraintName("FK_task_histories_users_UserId").OnDelete(DeleteBehavior.SetNull);
    }
}

