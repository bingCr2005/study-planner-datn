using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class PhienTapTrungConfiguration : IEntityTypeConfiguration<PhienTapTrung>
{
    public void Configure(EntityTypeBuilder<PhienTapTrung> b)
    {
        b.ToTable("phien_tap_trung", table =>
        {
            table.HasCheckConstraint("CK_focus_mode", "che_do IN ('Pomodoro', 'Stopwatch') AND muc_tieu_giay BETWEEN 60 AND 10800");
            table.HasCheckConstraint("CK_focus_state", "trang_thai IN ('Running', 'Paused', 'Completed', 'Discarded') AND so_mili_giay_da_hoc >= 0 AND ((trang_thai = 'Running' AND bat_dau_doan IS NOT NULL AND ket_thuc IS NULL) OR (trang_thai = 'Paused' AND bat_dau_doan IS NULL AND ket_thuc IS NULL) OR (trang_thai IN ('Completed', 'Discarded') AND bat_dau_doan IS NULL AND ket_thuc IS NOT NULL))");
            table.HasCheckConstraint("CK_focus_times", "(ket_thuc IS NULL OR ket_thuc >= bat_dau) AND (bat_dau_doan IS NULL OR bat_dau_doan >= bat_dau)");
        });
        b.HasKey(x => x.Ma);
        b.Property(x => x.Ma).UseIdentityByDefaultColumn();
        b.HasAlternateKey(x => new { x.Ma, x.MaNguoiDung });
        b.Property(x => x.TieuDe).HasMaxLength(500);
        b.Property(x => x.CheDo).HasMaxLength(16);
        b.Property(x => x.TrangThai).HasMaxLength(16);
        b.Property(x => x.GhiChu).HasMaxLength(2000);
        b.Property(x => x.LichSuSuaThoiGianJson).HasColumnType("jsonb");
        b.HasIndex(x => x.MaNguoiDung).IsUnique().HasFilter("trang_thai IN ('Running', 'Paused')");
        b.HasIndex(x => new { x.MaNguoiDung, x.BatDau });
        b.HasIndex(x => new { x.MaNguoiDung, x.MaPhienTuHoc }).IsUnique().HasFilter("ma_phien_tu_hoc IS NOT NULL AND trang_thai <> 'Discarded'");
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.NhiemVu).WithMany().HasForeignKey(x => x.MaNhiemVu).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.PhienTuHoc).WithMany().HasForeignKey(x => x.MaPhienTuHoc).OnDelete(DeleteBehavior.SetNull);
        foreach (var property in b.Metadata.GetProperties())
            property.SetColumnName(Regex.Replace(property.Name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant());
    }
}
