using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public sealed class NhacKeHoachConfiguration : IEntityTypeConfiguration<NhacKeHoach>
{
    public void Configure(EntityTypeBuilder<NhacKeHoach> builder)
    {
        builder.ToTable("nhac_ke_hoach", table => {
            table.HasCheckConstraint("CK_nhac_ke_hoach_loai", "loai IN ('WeeklyReview', 'AvailabilityExpiring')");
            table.HasCheckConstraint("CK_nhac_ke_hoach_noi_dung", "length(btrim(tieu_de)) > 0 AND length(btrim(noi_dung)) > 0 AND length(btrim(khoa_chong_trung)) > 0");
            table.HasCheckConstraint("CK_nhac_ke_hoach_doc", "ngay_doc IS NULL OR ngay_doc >= ngay_phat");
            table.HasCheckConstraint("CK_nhac_ke_hoach_duong_dan", "duong_dan IN ('/tasks?tab=planning', '/tasks?tab=free')");
        });
        builder.HasKey(x => x.Ma);
        builder.Property(x => x.Ma).HasColumnName("ma").UseIdentityByDefaultColumn();
        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(x => x.KhoaChongTrung).HasColumnName("khoa_chong_trung").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Loai).HasColumnName("loai").HasMaxLength(32).IsRequired();
        builder.Property(x => x.TieuDe).HasColumnName("tieu_de").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NoiDung).HasColumnName("noi_dung").HasMaxLength(2000).IsRequired();
        builder.Property(x => x.DuongDan).HasColumnName("duong_dan").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NgayPhat).HasColumnName("ngay_phat");
        builder.Property(x => x.NgayDoc).HasColumnName("ngay_doc");
        builder.HasIndex(x => new { x.MaNguoiDung, x.KhoaChongTrung }).IsUnique();
        builder.HasIndex(x => new { x.MaNguoiDung, x.NgayPhat });
        builder.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Cascade);
    }
}
