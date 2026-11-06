using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class LopSinhVienConfiguration : IEntityTypeConfiguration<LopSinhVien>
{
    public void Configure(EntityTypeBuilder<LopSinhVien> builder)
    {
        // Tên bảng/cột tiếng Việt; giữ tên constraint/index cũ để migration chỉ rename bảng.
        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaKhoa).HasColumnName("ma_khoa");
        builder.Property(x => x.MaKhoaHoc).HasColumnName("ma_khoa_hoc");
        builder.Property(x => x.Ten).HasColumnName("ten");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");

        builder.ToTable("lop_sinh_vien", table =>
        {
            table.HasCheckConstraint("CK_student_classes_name_not_blank", "length(btrim(\"ten\")) > 0");
        });
        builder.HasKey(x => x.Ma).HasName("PK_student_classes");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.MaKhoa, x.MaKhoaHoc, x.Ten }).IsUnique();
        builder.HasOne(x => x.Khoa).WithMany(x => x.DanhSachLopSinhVien).HasForeignKey(x => x.MaKhoa).HasConstraintName("FK_student_classes_faculties_FacultyId").OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.KhoaHoc).WithMany(x => x.DanhSachLopSinhVien).HasForeignKey(x => x.MaKhoaHoc).HasConstraintName("FK_student_classes_cohorts_CohortId").OnDelete(DeleteBehavior.Restrict);
        // Giữ tên index hiện có để migration chỉ đổi tên cột.
        builder.HasIndex(x => x.MaKhoaHoc).HasDatabaseName("IX_student_classes_CohortId");
        builder.HasIndex(x => new { x.MaKhoa, x.MaKhoaHoc, x.Ten }).HasDatabaseName("IX_student_classes_FacultyId_CohortId_Name");
    }
}

