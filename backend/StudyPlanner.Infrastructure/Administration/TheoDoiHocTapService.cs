using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Administration;

public sealed class TheoDoiHocTapQuery
{
    [Range(1, int.MaxValue)] public int? MaSinhVien { get; init; }
    [StringLength(200)] public string? TuKhoa { get; init; }
    [Range(1, int.MaxValue)] public int? MaHocKy { get; init; }
    [Range(1, int.MaxValue)] public int? MaMonHoc { get; init; }
    public TrangThaiNhiemVu? TrangThai { get; init; }
    public bool ChiQuaHan { get; init; }
    public bool GomBuocNho { get; init; }
    [Range(1, int.MaxValue)] public int Trang { get; init; } = 1;
    [Range(1, 100)] public int KichThuocTrang { get; init; } = 20;
}

// DTO quản trị có danh sách trường cho phép riêng, không tái sử dụng DTO của sinh viên.
// Vì vậy mô tả, checklist, tệp đính kèm, giờ rảnh và thông tin xác thực không thể
// vô tình lọt ra ngoài khi DTO nhiệm vụ của sinh viên được bổ sung trường mới.
public sealed record NhiemVuTheoDoi(int Ma, int MaSinhVien, string HoTen, string? MaSoSinhVien,
    int? MaHocKy, string? TenHocKy, int? MaMonHoc, string? TenMonHoc, int? MaNhiemVuCha,
    string TieuDe, TrangThaiNhiemVu TrangThai, MucDoUuTienNhiemVu MucDoUuTien,
    int TienDo, DateTimeOffset? HanHieuLuc, DateTimeOffset? NgayHoanThanh, bool CoBuocNho);
public sealed record TongQuanHocTap(int TongNhiemVu, int HoanThanh, int DangLam, int ChuaBatDau,
    int QuaHan, int SoSinhVienCoNhiemVu, DateTimeOffset KiemTraLuc);
public sealed record PhamViTheoDoi(int Ma, string Ten, int? MaHocKy);
public sealed record HoSoTheoDoi(int Ma, string HoTen, string? MaSoSinhVien, bool DangHoatDong,
    string MuiGio, IReadOnlyList<PhamViTheoDoi> HocKy, IReadOnlyList<PhamViTheoDoi> MonHoc);
public sealed record PhienTheoDoi(int Ma, int MaNhiemVu, string TieuDeNhiemVu,
    DateTimeOffset BatDauDuKien, DateTimeOffset KetThucDuKien, TrangThaiPhienTuHoc TrangThai);
public sealed record TongHopThoiGianHoc(int SoPhien, long TongMiliGiay, long BamGioMiliGiay, long NhapTayMiliGiay);

public sealed class TheoDoiHocTapService(ApplicationDbContext db, TimeProvider clock)
{
    private IQueryable<NhiemVu> HocTap() => db.DanhSachNhiemVu.AsNoTracking().Where(n =>
        n.NguoiDung.VaiTro == VaiTroNguoiDung.Student && n.Loai != LoaiNhiemVu.Personal &&
        (n.MaHocKy != null || n.MaMonHoc != null));

    private static void KiemTra(TheoDoiHocTapQuery q)
    {
        // Kiểm tra cả khi service được gọi trực tiếp, không chỉ dựa vào MVC.
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(q, new ValidationContext(q), errors, true))
            throw new HocTapException(400, "VALIDATION_ERROR", string.Join(" ", errors.Select(e => e.ErrorMessage)));
        if ((long)(q.Trang - 1) * q.KichThuocTrang > int.MaxValue ||
            (q.TrangThai.HasValue && !Enum.IsDefined(q.TrangThai.Value)))
            throw new HocTapException(400, "VALIDATION_ERROR", "Trang hoặc trạng thái không hợp lệ.");
    }

    public async Task KiemTraSinhVienAsync(int id, CancellationToken ct)
    {
        // Tài khoản bị khóa vẫn cần được admin theo dõi. Luồng này chỉ đọc và không
        // nới kiểm tra chủ sở hữu/trạng thái trong các API học tập của sinh viên.
        if (!await db.DanhSachNguoiDung.AnyAsync(u => u.Ma == id && u.VaiTro == VaiTroNguoiDung.Student, ct))
            throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy sinh viên.");
    }

    private async Task<IQueryable<NhiemVu>> LocAsync(TheoDoiHocTapQuery q, CancellationToken ct)
    {
        KiemTra(q);
        if (q.MaSinhVien is int student) await KiemTraSinhVienAsync(student, ct);
        if (q.MaHocKy is int semester && !await db.DanhSachHocKy.AnyAsync(s => s.Ma == semester &&
            s.NguoiDung.VaiTro == VaiTroNguoiDung.Student && (!q.MaSinhVien.HasValue || s.MaNguoiDung == q.MaSinhVien), ct))
            throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Học kỳ không thuộc phạm vi sinh viên.");
        if (q.MaMonHoc is int course && !await db.DanhSachMonHoc.AnyAsync(c => c.Ma == course &&
            c.HocKy.NguoiDung.VaiTro == VaiTroNguoiDung.Student &&
            (!q.MaSinhVien.HasValue || c.HocKy.MaNguoiDung == q.MaSinhVien) &&
            (!q.MaHocKy.HasValue || c.MaHocKy == q.MaHocKy), ct))
            throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Môn học không thuộc phạm vi đã chọn.");
        var query = HocTap();
        if (q.MaSinhVien.HasValue) query = query.Where(n => n.MaNguoiDung == q.MaSinhVien);
        if (q.MaHocKy.HasValue) query = query.Where(n => n.MaHocKy == q.MaHocKy);
        if (q.MaMonHoc.HasValue) query = query.Where(n => n.MaMonHoc == q.MaMonHoc);
        if (!q.GomBuocNho) query = query.Where(n => n.MaNhiemVuCha == null);
        if (q.TrangThai.HasValue) query = query.Where(n => n.TrangThai == q.TrangThai);
        var keyword = q.TuKhoa?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            var upper = keyword.ToUpperInvariant();
            query = query.Where(n => n.TieuDe.Contains(keyword) || n.TieuDe.ToUpper().Contains(upper) ||
                n.NguoiDung.HoTen.Contains(keyword) || n.NguoiDung.HoTen.ToUpper().Contains(upper) ||
                (n.NguoiDung.MaSoSinhVien != null && n.NguoiDung.MaSoSinhVien.ToUpper().Contains(upper)));
        }
        if (q.ChiQuaHan) query = QuyTacCay.LocHan(query.Where(n => n.TrangThai != TrangThaiNhiemVu.Completed), null, clock.GetUtcNow());
        return query;
    }

    public async Task<TongQuanHocTap> TongQuanAsync(TheoDoiHocTapQuery q, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var query = await LocAsync(q, ct);
        var now = clock.GetUtcNow();
        var counts = await query.GroupBy(_ => 1).Select(g => new {
            Total = g.Count(), Done = g.Count(n => n.TrangThai == TrangThaiNhiemVu.Completed),
            Doing = g.Count(n => n.TrangThai == TrangThaiNhiemVu.InProgress),
            Pending = g.Count(n => n.TrangThai == TrangThaiNhiemVu.NotStarted)
        }).SingleOrDefaultAsync(ct);
        var overdue = await QuyTacCay.LocHan(query.Where(n => n.TrangThai != TrangThaiNhiemVu.Completed), null, now).CountAsync(ct);
        var students = await query.Select(n => n.MaNguoiDung).Distinct().CountAsync(ct);
        await tx.CommitAsync(ct);
        return new(counts?.Total ?? 0, counts?.Done ?? 0, counts?.Doing ?? 0, counts?.Pending ?? 0, overdue, students, now);
    }

    private async Task<IReadOnlyList<NhiemVuTheoDoi>> DocNhiemVuAsync(IOrderedQueryable<NhiemVu> query, CancellationToken ct)
    {
        var rows = await query.Select(n => new NhiemVuTheoDoi(n.Ma, n.MaNguoiDung, n.NguoiDung.HoTen, n.NguoiDung.MaSoSinhVien,
            n.MaHocKy, n.HocKy == null ? null : n.HocKy.Ten, n.MaMonHoc, n.MonHoc == null ? null : n.MonHoc.Ten,
            n.MaNhiemVuCha, n.TieuDe, n.TrangThai, n.MucDoUuTien, n.TienDo, null, n.NgayHoanThanh, n.DanhSachNhiemVuCon.Any())).ToListAsync(ct);
        // Hai SELECT cùng ảnh chụp RepeatableRead và cùng thứ tự phân trang.
        // Tái dùng biểu thức SQL tính hạn kế thừa, tránh tải từng cây nhiệm vụ
        // và tránh trường hợp hạn/bản ghi lệch nhau khi sinh viên sửa đồng thời.
        var deadlines = await query.Select(QuyTacCay.BieuThucHan).ToListAsync(ct);
        return rows.Select((n, i) => n with { HanHieuLuc = deadlines[i] }).ToArray();
    }

    public async Task<PhanTrangResponse<NhiemVuTheoDoi>> NhiemVuAsync(TheoDoiHocTapQuery q, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var query = await LocAsync(q, ct);
        var total = await query.CountAsync(ct);
        var page = query.OrderByDescending(n => n.NgayTao).ThenBy(n => n.Ma)
            .Skip((q.Trang - 1) * q.KichThuocTrang).Take(q.KichThuocTrang).OrderByDescending(n => n.NgayTao).ThenBy(n => n.Ma);
        var rows = await DocNhiemVuAsync(page, ct);
        await tx.CommitAsync(ct);
        return new(rows, q.Trang, q.KichThuocTrang, total);
    }

    public async Task<HoSoTheoDoi> HoSoAsync(int id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await KiemTraSinhVienAsync(id, ct);
        var user = await db.DanhSachNguoiDung.AsNoTracking().Where(u => u.Ma == id)
            .Select(u => new { u.Ma, u.HoTen, u.MaSoSinhVien, u.DangHoatDong, u.MuiGio }).SingleAsync(ct);
        var semesters = await db.DanhSachHocKy.AsNoTracking().Where(s => s.MaNguoiDung == id).OrderByDescending(s => s.NgayBatDau).ThenBy(s => s.Ma)
            .Select(s => new PhamViTheoDoi(s.Ma, s.Ten, null)).ToListAsync(ct);
        var courses = await db.DanhSachMonHoc.AsNoTracking().Where(c => c.HocKy.MaNguoiDung == id).OrderBy(c => c.Ma)
            .Select(c => new PhamViTheoDoi(c.Ma, c.Ten, c.MaHocKy)).ToListAsync(ct);
        await tx.CommitAsync(ct);
        return new(user.Ma, user.HoTen, user.MaSoSinhVien, user.DangHoatDong, user.MuiGio, semesters, courses);
    }

    public async Task<PhanTrangResponse<PhienTheoDoi>> KeHoachAsync(int studentId, DateTimeOffset tu, DateTimeOffset den, int trang, CancellationToken ct)
    {
        KiemTraKhoang(tu, den, trang);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await KiemTraSinhVienAsync(studentId, ct);
        var query = db.DanhSachPhienTuHoc.AsNoTracking().Where(p => p.MaNguoiDung == studentId &&
            p.NhiemVu.MaNguoiDung == studentId && p.NhiemVu.Loai != LoaiNhiemVu.Personal &&
            (p.NhiemVu.MaHocKy != null || p.NhiemVu.MaMonHoc != null) &&
            p.BatDauDuKien < den && p.KetThucDuKien > tu);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(p => p.BatDauDuKien).ThenBy(p => p.Ma).Skip((trang - 1) * 20).Take(20)
            .Select(p => new PhienTheoDoi(p.Ma, p.MaNhiemVu, p.NhiemVu.TieuDe, p.BatDauDuKien, p.KetThucDuKien, p.TrangThai)).ToListAsync(ct);
        await tx.CommitAsync(ct);
        return new(rows, trang, 20, total);
    }

    public async Task<TongHopThoiGianHoc> ThoiGianAsync(int studentId, DateTimeOffset tu, DateTimeOffset den, CancellationToken ct)
    {
        KiemTraKhoang(tu, den, 1);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        await KiemTraSinhVienAsync(studentId, ct);
        // Chỉ cộng bản ghi đã kết thúc trong [từ, đến), không lấy giờ dự kiến
        // thay cho giờ thực học. Loại phiên tự do, nhiệm vụ cá nhân và thùng rác.
        // Đây là thời gian sinh viên ghi nhận; bản nhập tay được cộng riêng để
        // admin không hiểu nhầm tất cả thời lượng đều được đồng hồ đo tự động.
        var query = db.DanhSachPhienTapTrung.AsNoTracking().Where(p => p.MaNguoiDung == studentId && p.NhiemVu != null &&
            p.NhiemVu.MaNguoiDung == studentId && p.NhiemVu.Loai != LoaiNhiemVu.Personal &&
            (p.NhiemVu.MaHocKy != null || p.NhiemVu.MaMonHoc != null) &&
            p.NhiemVu.NgayXoa == null && p.TrangThai == "Completed" && p.KetThuc >= tu && p.KetThuc < den);
        var summary = await query.GroupBy(_ => 1).Select(g => new TongHopThoiGianHoc(g.Count(), g.Sum(p => p.SoMiliGiayDaHoc),
            g.Sum(p => p.NhapThuCong ? 0L : p.SoMiliGiayDaHoc), g.Sum(p => p.NhapThuCong ? p.SoMiliGiayDaHoc : 0L))).SingleOrDefaultAsync(ct);
        await tx.CommitAsync(ct);
        return summary ?? new(0, 0, 0, 0);
    }

    private static void KiemTraKhoang(DateTimeOffset tu, DateTimeOffset den, int trang)
    {
        if (trang < 1 || (long)(trang - 1) * 20 > int.MaxValue || den <= tu || den - tu > TimeSpan.FromDays(93))
            throw new HocTapException(400, "VALIDATION_ERROR", "Chọn khoảng từ trên 0 đến 93 ngày và trang hợp lệ.");
    }
}
