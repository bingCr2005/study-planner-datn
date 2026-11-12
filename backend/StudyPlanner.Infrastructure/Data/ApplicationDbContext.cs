using Microsoft.EntityFrameworkCore;
using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // Legacy tổ chức: ngoài chức năng cốt lõi, vẫn ánh xạ để bảo toàn DB/backfill hồ sơ cũ.
    // Không bỏ DbSet/configuration khi chưa có migration và kế hoạch chuyển dữ liệu tương ứng.
    public DbSet<Truong> DanhSachTruong => Set<Truong>();

    public DbSet<Khoa> DanhSachKhoa => Set<Khoa>();
    public DbSet<KhoaHoc> DanhSachKhoaHoc => Set<KhoaHoc>();
    public DbSet<LopSinhVien> DanhSachLopSinhVien => Set<LopSinhVien>();

    // Dữ liệu cốt lõi dành cho sinh viên.
    public DbSet<NguoiDung> DanhSachNguoiDung => Set<NguoiDung>();
    public DbSet<HocKy> DanhSachHocKy => Set<HocKy>();
    public DbSet<MonHoc> DanhSachMonHoc => Set<MonHoc>();
    public DbSet<LichHoc> DanhSachLichHoc => Set<LichHoc>();
    public DbSet<NhiemVu> DanhSachNhiemVu => Set<NhiemVu>();
    public DbSet<ThongBao> DanhSachThongBao => Set<ThongBao>();
    public DbSet<LichSuNhiemVu> DanhSachLichSuNhiemVu => Set<LichSuNhiemVu>();
    public DbSet<KhungGioRanh> DanhSachKhungGioRanh => Set<KhungGioRanh>();
    public DbSet<PhienTuHoc> DanhSachPhienTuHoc => Set<PhienTuHoc>();
    public DbSet<LanApDungLich> DanhSachLanApDungLich => Set<LanApDungLich>();
    public DbSet<LichNhac> DanhSachLichNhac => Set<LichNhac>();
    public DbSet<TepDinhKemNhiemVu> DanhSachTepDinhKemNhiemVu => Set<TepDinhKemNhiemVu>();
    public DbSet<DoanThucHoc> DanhSachDoanThucHoc => Set<DoanThucHoc>();
    public DbSet<QuyTacUuTien> DanhSachQuyTacUuTien => Set<QuyTacUuTien>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nạp mọi IEntityTypeConfiguration trong Infrastructure; chỉ xây dựng model,
        // không tạo bảng hoặc áp dụng migration khi ứng dụng khởi động.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<PhienTuHoc>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<DoanThucHoc>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<LichNhac>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<ThongBao>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<TepDinhKemNhiemVu>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
    }
}
