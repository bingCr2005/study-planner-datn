using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public sealed class NgoaiLeGioRanhConfiguration : IEntityTypeConfiguration<NgoaiLeGioRanh>
{
    public void Configure(EntityTypeBuilder<NgoaiLeGioRanh> b)
    {
        b.ToTable("ngoai_le_gio_ranh", t => t.HasCheckConstraint("CK_ngoai_le_gio_ranh_times",
            "(bo_qua AND gio_bat_dau IS NULL AND gio_ket_thuc IS NULL) OR (NOT bo_qua AND gio_bat_dau IS NOT NULL AND gio_ket_thuc IS NOT NULL AND gio_ket_thuc > gio_bat_dau)"));
        b.HasKey(x => x.Ma); b.Property(x => x.Ma).UseIdentityByDefaultColumn();
        b.HasOne(x => x.KhungGioRanh).WithMany().HasForeignKey(x => new { x.MaKhungGioRanh, x.MaNguoiDung })
            .HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.MaKhungGioRanh, x.Ngay }).IsUnique();
        b.HasIndex(x => new { x.MaNguoiDung, x.Ngay });
        foreach (var p in b.Metadata.GetProperties()) p.SetColumnName(System.Text.RegularExpressions.Regex.Replace(p.Name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant());
    }
}
