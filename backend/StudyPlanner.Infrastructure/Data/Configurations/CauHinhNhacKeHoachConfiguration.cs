using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public sealed class CauHinhNhacKeHoachConfiguration : IEntityTypeConfiguration<CauHinhNhacKeHoach>
{
    public void Configure(EntityTypeBuilder<CauHinhNhacKeHoach> builder)
    {
        builder.ToTable("cau_hinh_nhac_ke_hoach");
        builder.HasKey(x => x.MaNguoiDung);
        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung").ValueGeneratedNever();
        builder.Property(x => x.DangBat).HasColumnName("dang_bat").HasDefaultValue(false);
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.HasOne(x => x.NguoiDung).WithOne().HasForeignKey<CauHinhNhacKeHoach>(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Cascade);
    }
}
