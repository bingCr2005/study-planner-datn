using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {

        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(x => x.Ten).HasColumnName("ten");
        builder.Property(x => x.NgayBatDau).HasColumnName("ngay_bat_dau");
        builder.Property(x => x.NgayKetThuc).HasColumnName("ngay_ket_thuc");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("semesters", table =>
        {
            table.HasCheckConstraint("CK_semesters_dates", "\"ngay_ket_thuc\" >= \"ngay_bat_dau\"");
            table.HasCheckConstraint("CK_semesters_name_not_blank", "length(btrim(\"ten\")) > 0");
        });
        builder.HasKey(x => x.Ma).HasName("PK_semesters");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayBatDau).HasColumnType("date");
        builder.Property(x => x.NgayKetThuc).HasColumnType("date");
        builder.HasIndex(x => new { x.MaNguoiDung, x.NgayBatDau }).HasDatabaseName("IX_semesters_UserId_StartDate");
        builder.HasOne(x => x.User).WithMany(x => x.Semesters).HasForeignKey(x => x.MaNguoiDung).HasConstraintName("FK_semesters_users_UserId").OnDelete(DeleteBehavior.Cascade);
    }
}

