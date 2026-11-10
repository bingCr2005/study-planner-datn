using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;

public sealed class KhungGioRanhService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, ThoiKhoaBieuService calendar, TimeProvider clock)
{
    public async Task<HocTapResponse<IReadOnlyList<KhungGioRanhResponse>>> DanhSachAsync(int chu, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        var rows = await TaiAsync(chu, ct);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, rows.OrderBy(x => x.ThuTrongTuan).ThenBy(x => x.GioBatDau).ThenBy(x => x.Ma).Select(Dto).ToArray());
    }

    public async Task<HocTapResponse<KhungGioRanhResponse>> ChiTietAsync(int chu, int ma, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        var row = await TimAsync(chu, ma, ct);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, Dto(row));
    }

    public async Task<HocTapResponse<KhungGioRanhResponse>> LuuAsync(int chu, int? ma, KhungGioRanhRequest request, long revision, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var zone = QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio);
        KiemTra(request, zone);
        var row = ma.HasValue ? await TimAsync(chu, ma.Value, ct) : new KhungGioRanh { MaNguoiDung = chu };
        var changed = !ma.HasValue || !Giong(row, request);
        if (changed)
        {
            var now = clock.GetUtcNow();
            var proposed = new KhungGioRanh { Ma = ma ?? 0, MaNguoiDung = chu }; Gan(proposed, request);
            var (impact, affected) = await TinhAnhHuongAsync(chu, ma, proposed, zone, now, ct);
            QuyTacAnhHuongLich.ApDung(affected, impact, request.HuyPhienLienQuan, now);
            Gan(row, request); row.NgayCapNhat = now;
            if (!ma.HasValue) { row.NgayTao = now; store.Them(row); }
        }
        await session.CommitAsync(changed, ct);
        return new(session.NguoiDung.PhienBanLapLich, Dto(row));
    }

    public async Task<long> XoaAsync(int chu, int ma, long revision, bool huyPhienLienQuan, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var row = await TimAsync(chu, ma, ct);
        var now = clock.GetUtcNow();
        var (impact, affected) = await TinhAnhHuongAsync(chu, ma, null, QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio), now, ct);
        QuyTacAnhHuongLich.ApDung(affected, impact, huyPhienLienQuan, now);
        store.Xoa(row); await session.CommitAsync(true, ct);
        return session.NguoiDung.PhienBanLapLich;
    }

    public async Task<HocTapResponse<AnhHuongLichResponse>> XemAnhHuongAsync(int chu, int? ma, KhungGioRanhRequest? request, bool xoa, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        if (ma.HasValue) await TimAsync(chu, ma.Value, ct);
        else if (xoa) throw new HocTapException(400, "VALIDATION_ERROR", "Xóa cần mã khung giờ.");
        var zone = QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio);
        KhungGioRanh? proposed = null;
        if (!xoa)
        {
            if (request is null) throw new HocTapException(400, "VALIDATION_ERROR", "Thiếu khung giờ rảnh.");
            KiemTra(request, zone);
            proposed = new() { Ma = ma ?? 0, MaNguoiDung = chu }; Gan(proposed, request);
        }
        var (impact, _) = await TinhAnhHuongAsync(chu, ma, proposed, zone, clock.GetUtcNow(), ct);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, impact);
    }

    private async Task<(AnhHuongLichResponse Impact, List<PhienTuHoc> Affected)> TinhAnhHuongAsync(int chu, int? ma, KhungGioRanh? proposed, TimeZoneInfo zone, DateTimeOffset now, CancellationToken ct)
    {
        // Chỉ phiên nằm trong khung cũ bị thay đổi có thể mất tính hợp lệ.
        // Phiên ngoài các khung cũ (legacy) không bị hủy chỉ vì người dùng thêm một khung mới.
        if (!ma.HasValue) return (new([], [], false), []);
        var all = await TaiAsync(chu, ct);
        var old = all.Single(x => x.Ma == ma);
        var updated = all.Where(x => x.Ma != ma).ToList();
        if (proposed is not null) updated.Add(proposed);
        var sessions = await QuyTacAnhHuongLich.LayPhienAsync(store, chu, now, ct);
        var affected = sessions.Where(x =>
        {
            var k = new KhoangThoiGian(x.BatDauDuKien, x.KetThucDuKien);
            if (!QuyTacThoiGian.SinhGioRanh(old, zone, k.BatDau, k.KetThuc).Any(b => QuyTacThoiGian.Giao(b, k))) return false;
            var remaining = QuyTacThoiGian.HopNhat(updated.SelectMany(r => QuyTacThoiGian.SinhGioRanh(r, zone, k.BatDau, k.KetThuc)));
            return !remaining.Any(b => b.BatDau <= k.BatDau && b.KetThuc >= k.KetThuc);
        }).ToList();
        return (new(affected.Select(x => x.Ma).ToArray(), [], await QuyTacAnhHuongLich.DangHocAsync(store, chu, affected, now, ct)), affected);
    }

    private Task<List<KhungGioRanh>> TaiAsync(int chu, CancellationToken ct) => calendar.TaiCoGioiHanAsync(store.TruyVan<KhungGioRanh>().Where(x => x.MaNguoiDung == chu), ct);
    private async Task<KhungGioRanh> TimAsync(int chu, int ma, CancellationToken ct) =>
        await store.DauTienAsync(store.TruyVan<KhungGioRanh>().Where(x => x.Ma == ma && x.MaNguoiDung == chu), ct)
        ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy khung giờ rảnh.");

    private static void KiemTra(KhungGioRanhRequest r, TimeZoneInfo zone)
    {
        if (r.ThuTrongTuan is < 1 or > 7 || r.GioKetThuc <= r.GioBatDau || r.NgayBatDau == default || r.NgayKetThuc == default ||
            r.NgayKetThuc < r.NgayBatDau || !QuyTacThoiGian.CoNgay(r.NgayBatDau, r.NgayKetThuc, r.ThuTrongTuan))
            throw new HocTapException(400, "VALIDATION_ERROR", "Thứ, giờ hoặc ngày hiệu lực của khung giờ rảnh không hợp lệ.");
        var test = new KhungGioRanh { DangSuDung = true }; Gan(test, r); test.DangSuDung = true;
        _ = QuyTacThoiGian.SinhGioRanh(test, zone, QuyTacThoiGian.Utc(r.NgayBatDau, TimeOnly.MinValue, zone), QuyTacThoiGian.Utc(r.NgayKetThuc, TimeOnly.MaxValue, zone));
    }

    private static void Gan(KhungGioRanh e, KhungGioRanhRequest r)
    { e.ThuTrongTuan = r.ThuTrongTuan; e.GioBatDau = r.GioBatDau; e.GioKetThuc = r.GioKetThuc; e.NgayBatDau = r.NgayBatDau; e.NgayKetThuc = r.NgayKetThuc; e.DangSuDung = r.DangSuDung; }
    private static bool Giong(KhungGioRanh e, KhungGioRanhRequest r) => e.ThuTrongTuan == r.ThuTrongTuan && e.GioBatDau == r.GioBatDau && e.GioKetThuc == r.GioKetThuc && e.NgayBatDau == r.NgayBatDau && e.NgayKetThuc == r.NgayKetThuc && e.DangSuDung == r.DangSuDung;
    private static KhungGioRanhResponse Dto(KhungGioRanh e) => new(e.Ma, e.ThuTrongTuan, e.GioBatDau, e.GioKetThuc, e.NgayBatDau, e.NgayKetThuc, e.DangSuDung, e.NgayTao, e.NgayCapNhat);
}
