using Microsoft.EntityFrameworkCore;
using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // Tổ chức cũ ngoài chức năng cốt lõi vẫn được ánh xạ để bảo toàn cơ sở dữ liệu và chuyển hồ sơ cũ.
    // Không bỏ DbSet/cấu hình khi chưa có bản chuyển đổi và kế hoạch chuyển dữ liệu tương ứng.
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
    public DbSet<NgoaiLeGioRanh> DanhSachNgoaiLeGioRanh => Set<NgoaiLeGioRanh>();
    public DbSet<CauHinhNhacKeHoach> DanhSachCauHinhNhacKeHoach => Set<CauHinhNhacKeHoach>();
    public DbSet<NhacKeHoach> DanhSachNhacKeHoach => Set<NhacKeHoach>();
    public DbSet<PhienTuHoc> DanhSachPhienTuHoc => Set<PhienTuHoc>();
    public DbSet<LanApDungLich> DanhSachLanApDungLich => Set<LanApDungLich>();
    public DbSet<LichNhac> DanhSachLichNhac => Set<LichNhac>();
    public DbSet<TepDinhKemNhiemVu> DanhSachTepDinhKemNhiemVu => Set<TepDinhKemNhiemVu>();
    public DbSet<KhoangThoiGianHoc> DanhSachKhoangThoiGianHoc => Set<KhoangThoiGianHoc>();
    public DbSet<PhienTapTrung> DanhSachPhienTapTrung => Set<PhienTapTrung>();
    public DbSet<QuyTacUuTien> DanhSachQuyTacUuTien => Set<QuyTacUuTien>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nạp mọi IEntityTypeConfiguration trong Infrastructure để xây dựng mô hình dữ liệu;
        // không tạo bảng hoặc áp dụng bản chuyển đổi khi ứng dụng khởi động.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<PhienTuHoc>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<KhoangThoiGianHoc>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<LichNhac>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<ThongBao>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
        modelBuilder.Entity<TepDinhKemNhiemVu>().HasQueryFilter(x => x.NhiemVu.NgayXoa == null);
    }
}
