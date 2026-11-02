using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {

        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaMonHoc).HasColumnName("ma_mon_hoc");
        builder.Property(x => x.ThuTrongTuan).HasColumnName("thu_trong_tuan");
        builder.Property(x => x.GioBatDau).HasColumnName("gio_bat_dau");
        builder.Property(x => x.GioKetThuc).HasColumnName("gio_ket_thuc");
        builder.Property(x => x.PhongHoc).HasColumnName("phong_hoc");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("lich_hoc", table =>
        {
            table.HasCheckConstraint("CK_class_sessions_weekday", "\"thu_trong_tuan\" BETWEEN 1 AND 7");
            table.HasCheckConstraint("CK_class_sessions_times", "\"gio_ket_thuc\" > \"gio_bat_dau\"");
        });
        builder.HasKey(x => x.Ma).HasName("PK_class_sessions");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.PhongHoc).HasMaxLength(200);
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        builder.Property(x => x.GioBatDau).HasColumnType("time without time zone");
        builder.Property(x => x.GioKetThuc).HasColumnType("time without time zone");
        builder.HasIndex(x => new { x.MaMonHoc, x.ThuTrongTuan, x.GioBatDau }).HasDatabaseName("IX_class_sessions_CourseId_DayOfWeek_StartTime");
        builder.HasOne(x => x.Course).WithMany(x => x.ClassSessions).HasForeignKey(x => x.MaMonHoc).HasConstraintName("FK_class_sessions_courses_CourseId").OnDelete(DeleteBehavior.Cascade);
    }
}

