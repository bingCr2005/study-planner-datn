using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.CanhBao;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.CanhBao;

public sealed record ThongBaoResponse(int Ma, int MaNhiemVu, string TieuDe, string TenNhiemVu,
    string NoiDung, DateTimeOffset? HanHieuLuc, int? MaHocKy, string? TenHocKy, int? MaMonHoc,
    string? TenMonHoc, bool QuaHan, bool DaHoanThanh, DateTimeOffset NgayPhat, DateTimeOffset? NgayDoc);
public sealed record HopThongBaoResponse(IReadOnlyList<ThongBaoResponse> DanhSach, int Trang,
    int KichThuocTrang, int TongSo, int TongChuaDoc, string MuiGio, DateTimeOffset DocDenLuc);

public sealed class ThongBaoService(ApplicationDbContext db, IHocTapStore store, TimeProvider clock)
{
    private async Task<Domain.Entities.NguoiDung> DongBoTrongGiaoDich(int owner, CancellationToken ct)
    {
        // Same per-user row lock as task writes: worker, reads and mutations serialize.
        var user = await store.NguoiDungAsync(owner, true, ct);
        var tasks = await db.DanhSachNhiemVu.Where(x => x.MaNguoiDung == owner).ToListAsync(ct);
        var rows = await db.DanhSachThongBao.Where(x => x.MaNguoiDung == owner).ToListAsync(ct);
        db.DanhSachThongBao.AddRange(QuyTacThongBao.DongBo(tasks, rows, user, clock.GetUtcNow()));
        await db.SaveChangesAsync(ct);
        return user;
    }

    public async Task DongBoAsync(int owner, CancellationToken ct)
    {
        await using var tx = await store.BatDauAsync(true, ct);
        await DongBoTrongGiaoDich(owner, ct);
        await tx.CommitAsync(ct);
    }

    public async Task<HopThongBaoResponse> DanhSachAsync(int owner, bool chuaDoc, int trang, int kichThuocTrang, CancellationToken ct)
    {
        if (trang < 1 || kichThuocTrang is < 1 or > 50 || (long)(trang - 1) * kichThuocTrang > int.MaxValue)
            throw new HocTapException(400, "VALIDATION_ERROR", "Phân trang thông báo không hợp lệ.");
        await using var tx = await store.BatDauAsync(true, ct);
        var user = await DongBoTrongGiaoDich(owner, ct);
        var query = db.DanhSachThongBao.Where(x => x.MaNguoiDung == owner && x.NhiemVu.MaNguoiDung == owner && x.TrangThaiPhat == TrangThaiPhatThongBao.Delivered);
        var unread = await query.CountAsync(x => x.NgayDoc == null, ct);
        if (chuaDoc) query = query.Where(x => x.NgayDoc == null);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.NgayPhat).ThenByDescending(x => x.Ma)
            .Skip((trang - 1) * kichThuocTrang).Take(kichThuocTrang)
            .Select(x => new ThongBaoResponse(x.Ma, x.MaNhiemVu, x.TieuDe, x.NhiemVu.TieuDe,
                x.NoiDung, null, x.NhiemVu.MaHocKy, x.NhiemVu.HocKy == null ? null : x.NhiemVu.HocKy.Ten,
                x.NhiemVu.MaMonHoc, x.NhiemVu.MonHoc == null ? null : x.NhiemVu.MonHoc.Ten,
                x.TieuDe == QuyTacThongBao.QuaHan, x.NhiemVu.TrangThai == TrangThaiNhiemVu.Completed,
                x.NgayPhat!.Value, x.NgayDoc)).ToListAsync(ct);
        var nodes = await db.DanhSachNhiemVu.Where(x => x.MaNguoiDung == owner).ToDictionaryAsync(x => x.Ma, ct);
        var result = rows.Select(x => x with { HanHieuLuc = QuyTacCay.HanHieuLuc(nodes[x.MaNhiemVu], nodes).Han }).ToArray();
        var cutoff = clock.GetUtcNow();
        await tx.CommitAsync(ct);
        return new(result, trang, kichThuocTrang, total, unread, user.MuiGio, cutoff);
    }

    public async Task DocAsync(int owner, int id, DateTimeOffset ngayPhat, CancellationToken ct)
    {
        await using var tx = await store.BatDauAsync(true, ct);
        await DongBoTrongGiaoDich(owner, ct);
        var row = await db.DanhSachThongBao.SingleOrDefaultAsync(x => x.Ma == id && x.MaNguoiDung == owner && x.NhiemVu.MaNguoiDung == owner && x.TrangThaiPhat == TrangThaiPhatThongBao.Delivered, ct)
            ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Thông báo không còn tồn tại.");
        if (row.NgayPhat != ngayPhat) throw new HocTapException(409, "NOTIFICATION_CHANGED", "Thông báo đã thay đổi. Hãy tải lại danh sách.");
        row.NgayDoc ??= clock.GetUtcNow() < row.NgayPhat ? row.NgayPhat : clock.GetUtcNow();
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task DocTatCaAsync(int owner, DateTimeOffset docDenLuc, CancellationToken ct)
    {
        if (docDenLuc > clock.GetUtcNow()) throw new HocTapException(400, "VALIDATION_ERROR", "Mốc đọc thông báo không hợp lệ.");
        await using var tx = await store.BatDauAsync(true, ct);
        await DongBoTrongGiaoDich(owner, ct);
        var now = clock.GetUtcNow();
        await db.DanhSachThongBao.Where(x => x.MaNguoiDung == owner && x.NhiemVu.MaNguoiDung == owner && x.TrangThaiPhat == TrangThaiPhatThongBao.Delivered && x.NgayDoc == null && x.NgayPhat <= docDenLuc)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.NgayDoc, now).SetProperty(x => x.NgayCapNhat, now), ct);
        await tx.CommitAsync(ct);
    }
}
