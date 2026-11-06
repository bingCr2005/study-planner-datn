using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

// Mapping của các bảng mới. Ràng buộc DB bảo vệ dữ liệu cơ bản;
// quyền, trùng lịch lặp, hạn cha-con và giao dịch apply vẫn phải thực hiện trong Application.
public sealed class LapKeHoachConfiguration : IEntityTypeConfiguration<KhungGioRanh>,
    IEntityTypeConfiguration<PhienTuHoc>, IEntityTypeConfiguration<LanApDungLich>,
    IEntityTypeConfiguration<LichNhac>, IEntityTypeConfiguration<TepDinhKemNhiemVu>,
    IEntityTypeConfiguration<DoanThucHoc>, IEntityTypeConfiguration<QuyTacUuTien>
{
    private static void Map<T>(EntityTypeBuilder<T> b, string table) where T : class
    {
        b.ToTable(table); b.HasKey("Ma"); b.Property<int>("Ma").UseIdentityByDefaultColumn();
        // Chỉ duyệt scalar EF đã nhận diện, không ánh xạ navigation thành cột.
        foreach (var p in b.Metadata.GetProperties())
            p.SetColumnName(System.Text.RegularExpressions.Regex.Replace(p.Name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant());
    }
    private static void Check<T>(EntityTypeBuilder<T> b, string table, string suffix, string sql) where T : class =>
        b.ToTable(table, t => t.HasCheckConstraint("CK_" + table + "_" + suffix, sql));

    public void Configure(EntityTypeBuilder<KhungGioRanh> b)
    {
        const string t = "khung_gio_ranh";
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.MaNguoiDung, x.ThuTrongTuan, x.GioBatDau });
        Check(b,t,"times", "thu_trong_tuan BETWEEN 1 AND 7 AND gio_ket_thuc > gio_bat_dau AND ngay_ket_thuc >= ngay_bat_dau");
        Map(b,t);
    }
    public void Configure(EntityTypeBuilder<PhienTuHoc> b)
    {
        const string t = "phien_tu_hoc";
        b.HasAlternateKey(x => new { x.Ma, x.MaNhiemVu, x.MaNguoiDung });
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NhiemVu).WithMany().HasForeignKey(x => new { x.MaNhiemVu, x.MaNguoiDung })
            .HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.LanApDung).WithMany().HasForeignKey(x => new { x.MaLanApDung, x.MaNguoiDung })
            .HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.NoAction);
        b.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => new { x.MaNguoiDung, x.BatDauDuKien });
        // Khóa tài khoản là điều phối chính; unique này là lớp bảo vệ DB cho hai lệnh Start cạnh tranh.
        b.HasIndex(x => x.MaNguoiDung).IsUnique().HasDatabaseName("UX_phien_tu_hoc_dang_hoc")
            .HasFilter("trang_thai = 'InProgress'");
        Check(b,t,"times", "ket_thuc_du_kien > bat_dau_du_kien");
        Check(b,t,"status", "trang_thai IN ('Planned','InProgress','Completed','Missed','Cancelled')");
        Map(b,t);
    }
    public void Configure(EntityTypeBuilder<LanApDungLich> b)
    {
        const string t = "lan_ap_dung_lich";
        b.HasAlternateKey(x => new { x.Ma, x.MaNguoiDung });
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.KhoaYeuCau).HasMaxLength(128).IsRequired();
        b.Property(x => x.DauKiemNoiDung).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.MaNguoiDung, x.KhoaYeuCau }).IsUnique();
        Check(b,t,"key", "length(btrim(khoa_yeu_cau)) > 0 AND dau_kiem_noi_dung ~ '^[0-9a-fA-F]{64}$' AND phien_ban_nguon >= 0 AND phien_ban_quy_tac > 0");
        Map(b,t);
    }
    public void Configure(EntityTypeBuilder<LichNhac> b)
    {
        const string t = "lich_nhac";
        b.HasAlternateKey(x => new { x.Ma, x.MaNguoiDung, x.MaNhiemVu });
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NhiemVu).WithMany().HasForeignKey(x => new { x.MaNhiemVu, x.MaNguoiDung })
            .HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.MocNhac).HasMaxLength(32).IsRequired(); b.Property(x => x.NoiDung).IsRequired();
        b.HasIndex(x => x.MaThongBaoCu).IsUnique().HasFilter("ma_thong_bao_cu IS NOT NULL");
        b.HasIndex(x => x.ThoiDiemNhac).HasFilter("trang_thai = 'Pending'");
        b.HasIndex(x => x.LanThuTiepTheoLuc).HasFilter("trang_thai = 'Failed'");
        Check(b,t,"values", "(nhac_truoc_phut IS NULL OR nhac_truoc_phut >= 0) AND so_lan_thu >= 0 AND length(btrim(moc_nhac)) > 0 AND length(btrim(noi_dung)) > 0");
        Check(b,t,"status", "trang_thai IN ('Pending','Delivered','Cancelled','Failed') AND ((trang_thai = 'Delivered' AND ngay_phat IS NOT NULL) OR (trang_thai <> 'Delivered' AND ngay_phat IS NULL))");
        Map(b,t);
    }
    public void Configure(EntityTypeBuilder<TepDinhKemNhiemVu> b)
    {
        const string t = "tep_dinh_kem_nhiem_vu";
        b.HasOne(x => x.NguoiTaiLen).WithMany().HasForeignKey(x => x.MaNguoiTaiLen).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NhiemVu).WithMany().HasForeignKey(x => new { x.MaNhiemVu, x.MaNguoiTaiLen })
            .HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.TenTep).HasMaxLength(255).IsRequired();
        b.Property(x => x.KhoaLuuTru).HasMaxLength(500).IsRequired();
        b.Property(x => x.LoaiTep).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.KhoaLuuTru).IsUnique();
        Check(b,t,"values", "kich_thuoc_byte > 0 AND length(btrim(ten_tep)) > 0 AND length(btrim(khoa_luu_tru)) > 0 AND length(btrim(loai_tep)) > 0");
        Map(b,t);
    }
    public void Configure(EntityTypeBuilder<DoanThucHoc> b)
    {
        const string t = "doan_thuc_hoc";
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NhiemVu).WithMany().HasForeignKey(x => new { x.MaNhiemVu, x.MaNguoiDung })
            .HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.PhienTuHoc).WithMany().HasForeignKey(x => new { x.MaPhienTuHoc, x.MaNhiemVu, x.MaNguoiDung })
            .HasPrincipalKey(x => new { x.Ma, x.MaNhiemVu, x.MaNguoiDung }).OnDelete(DeleteBehavior.NoAction);
        // Mỗi người chỉ một đoạn đang chạy. Chống khoảng thực học chồng lấn đã đóng cần dịch vụ.
        b.HasIndex(x => x.MaNguoiDung).IsUnique().HasFilter("ket_thuc_thuc_te IS NULL");
        Check(b,t,"times", "ket_thuc_thuc_te IS NULL OR ket_thuc_thuc_te > bat_dau_thuc_te");
        Map(b,t);
    }
    public void Configure(EntityTypeBuilder<QuyTacUuTien> b)
    {
        const string t = "quy_tac_uu_tien";
        b.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        b.Property(x => x.CauHinh).HasColumnType("jsonb").IsRequired();
        b.HasIndex(x => x.PhienBan).IsUnique();
        b.HasIndex(x => x.DangSuDung).IsUnique().HasFilter("dang_su_dung = TRUE");
        Check(b,t,"values", "phien_ban > 0 AND length(btrim(ten)) > 0 AND jsonb_typeof(cau_hinh) = 'object'");
        Map(b,t);
    }
}
