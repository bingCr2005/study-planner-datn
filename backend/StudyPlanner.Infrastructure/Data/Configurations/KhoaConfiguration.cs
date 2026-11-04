using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class KhoaConfiguration : IEntityTypeConfiguration<Khoa>
{
    public void Configure(EntityTypeBuilder<Khoa> builder)
    {
        // Tên bảng/cột tiếng Việt; giữ tên constraint/index cũ để migration chỉ rename bảng.
        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaTruong).HasColumnName("ma_truong");
        builder.Property(x => x.KyHieu).HasColumnName("ky_hieu");
        builder.Property(x => x.Ten).HasColumnName("ten");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");

        builder.ToTable("khoa", table =>
        {
            table.HasCheckConstraint("CK_faculties_code_not_blank", "length(btrim(\"ky_hieu\")) > 0");
            table.HasCheckConstraint("CK_faculties_name_not_blank", "length(btrim(\"ten\")) > 0");
        });

        builder.HasKey(x => x.Ma).HasName("PK_faculties");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();

        builder.Property(x => x.KyHieu)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Ten)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NgayTao)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.NgayCapNhat)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Mã khoa được lặp ở trường khác, nhưng không được trùng trong cùng trường.
        builder.HasIndex(x => new { x.MaTruong, x.KyHieu })
            .IsUnique();

        builder.HasOne(x => x.Truong)
            .WithMany(x => x.DanhSachKhoa)
            .HasForeignKey(x => x.MaTruong).HasConstraintName("FK_faculties_schools_SchoolId")
            .OnDelete(DeleteBehavior.Restrict); // Còn khoa thì không được xóa trường.
        // Giữ tên index hiện có để migration chỉ đổi tên cột.
        builder.HasIndex(x => new { x.MaTruong, x.KyHieu }).HasDatabaseName("IX_faculties_SchoolId_Code");
    }
}
