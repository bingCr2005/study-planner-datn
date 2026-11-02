using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasIndex(x => x.MaHocKy).HasDatabaseName("IX_courses_SemesterId");
        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaHocKy).HasColumnName("ma_hoc_ky");
        builder.Property(x => x.KyHieu).HasColumnName("ky_hieu");
        builder.Property(x => x.Ten).HasColumnName("ten");
        builder.Property(x => x.SoTinChi).HasColumnName("so_tin_chi");
        builder.Property(x => x.GiangVien).HasColumnName("giang_vien");
        builder.Property(x => x.MauSac).HasColumnName("mau_sac");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("mon_hoc", table =>
        {
            table.HasCheckConstraint("CK_courses_credits", "\"so_tin_chi\" IS NULL OR \"so_tin_chi\" > 0");
            table.HasCheckConstraint("CK_courses_color", "\"mau_sac\" IS NULL OR \"mau_sac\" ~ '^#[0-9A-Fa-f]{6}$'");
            table.HasCheckConstraint("CK_courses_name_not_blank", "length(btrim(\"ten\")) > 0");
        });
        builder.HasKey(x => x.Ma).HasName("PK_courses");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.KyHieu).HasMaxLength(50);
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GiangVien).HasMaxLength(200);
        builder.Property(x => x.MauSac).HasMaxLength(7);
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.Semester).WithMany(x => x.Courses).HasForeignKey(x => x.MaHocKy).HasConstraintName("FK_courses_semesters_SemesterId").OnDelete(DeleteBehavior.Cascade);
    }
}

