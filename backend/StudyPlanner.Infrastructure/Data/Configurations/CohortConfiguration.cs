using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class CohortConfiguration : IEntityTypeConfiguration<Cohort>
{
    public void Configure(EntityTypeBuilder<Cohort> builder)
    {

        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.Ten).HasColumnName("ten");
        builder.Property(x => x.NamBatDau).HasColumnName("nam_bat_dau");
        builder.Property(x => x.NamKetThuc).HasColumnName("nam_ket_thuc");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("khoa_hoc", table =>
        {
            table.HasCheckConstraint("CK_cohorts_years", "\"nam_ket_thuc\" >= \"nam_bat_dau\"");
            table.HasCheckConstraint("CK_cohorts_name_not_blank", "length(btrim(\"ten\")) > 0");
        });
        builder.HasKey(x => x.Ma).HasName("PK_cohorts");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.Ten).HasDatabaseName("IX_cohorts_Name").IsUnique();
    }
}

