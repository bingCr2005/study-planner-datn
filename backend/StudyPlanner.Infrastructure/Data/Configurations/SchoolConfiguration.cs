using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {

        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.KyHieu).HasColumnName("ky_hieu");
        builder.Property(x => x.Ten).HasColumnName("ten");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("truong", table =>
        {
            table.HasCheckConstraint("CK_schools_code_not_blank", "length(btrim(\"ky_hieu\")) > 0");
            table.HasCheckConstraint("CK_schools_name_not_blank", "length(btrim(\"ten\")) > 0");
        });

        builder.HasKey(x => x.Ma).HasName("PK_schools");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();

        builder.Property(x => x.KyHieu)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Ten)
            .HasMaxLength(200)
            .IsRequired();

        // Lưu thời điểm UTC. Service phải gán CreatedAt/UpdatedAt; không có trigger tự cập nhật.
        builder.Property(x => x.NgayTao)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.NgayCapNhat)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(x => x.KyHieu).HasDatabaseName("IX_schools_Code")
            .IsUnique();
    }
}
