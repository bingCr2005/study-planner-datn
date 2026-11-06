using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;

public sealed class LichHocService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, ThoiKhoaBieuService calendar, TimeProvider clock)
{
    public async Task<HocTapResponse<IReadOnlyList<LichHocResponse>>> DanhSachAsync(int chu, int mon, CancellationToken ct)
    {
        await using var phien = await dieuPhoi.MoAsync(chu, null, false, ct);
        var source = await MonAsync(chu, mon, ct);
        var rows = await calendar.TaiCoGioiHanAsync(store.TruyVan<LichHoc>().Where(x => x.MaMonHoc == mon)
            .OrderBy(x => x.ThuTrongTuan).ThenBy(x => x.GioBatDau).ThenBy(x => x.Ma), ct);
        await phien.CommitAsync(false, ct);
        return new(phien.NguoiDung.PhienBanLapLich, rows.Select(x => Dto(x, source.Mon.Ten)).ToArray());
    }

    public async Task<HocTapResponse<LichHocResponse>> ChiTietAsync(int chu, int ma, CancellationToken ct)
    {
        await using var phien = await dieuPhoi.MoAsync(chu, null, false, ct);
        var row = await TimAsync(chu, ma, ct);
        var mon = await MonAsync(chu, row.MaMonHoc, ct);
        await phien.CommitAsync(false, ct);
        return new(phien.NguoiDung.PhienBanLapLich, Dto(row, mon.Mon.Ten));
    }

    public async Task<HocTapResponse<LichHocResponse>> LuuAsync(int chu, int? ma, int? mon, LichHocRequest request, long revision, CancellationToken ct)
    {
        await using var phien = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var row = ma.HasValue ? await TimAsync(chu, ma.Value, ct) : new LichHoc { MaMonHoc = mon ?? throw new HocTapException(400, "VALIDATION_ERROR", "Thiếu mã môn.") };
        var source = await MonAsync(chu, row.MaMonHoc, ct);
        KiemTra(request, source.HocKy, QuyTacThoiGian.MuiGio(phien.NguoiDung.MuiGio));
        var changed = !ma.HasValue || !Giong(row, request);
        if (changed)
        {
            var now = clock.GetUtcNow();
            var proposed = Tao(row.MaMonHoc, request, row.Ma);
            var (impact, affected) = await TinhAnhHuongAsync(chu, proposed, source.HocKy, phien.NguoiDung.MuiGio, now, ct);
            if (impact.LichHocTrung.Count != 0 && !request.XacNhanTrungLich)
                throw new HocTapException(409, "CLASS_OVERLAP_CONFIRMATION_REQUIRED", "Lịch học trùng với lịch đã có. Hãy xác nhận nếu vẫn cần lưu.", impact);
            QuyTacAnhHuongLich.ApDung(affected, impact, request.HuyPhienLienQuan, now);
            Gan(row, request); row.NgayCapNhat = now;
            if (!ma.HasValue) { row.NgayTao = now; store.Them(row); }
        }
        await phien.CommitAsync(changed, ct);
        return new(phien.NguoiDung.PhienBanLapLich, Dto(row, source.Mon.Ten));
    }

    public async Task<long> XoaAsync(int chu, int ma, long revision, CancellationToken ct)
    {
        await using var phien = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var row = await TimAsync(chu, ma, ct);
        // Bỏ một khoảng bận không làm các phiên tự học hiện có vi phạm.
        store.Xoa(row); await phien.CommitAsync(true, ct);
        return phien.NguoiDung.PhienBanLapLich;
    }

    public async Task<HocTapResponse<AnhHuongLichResponse>> XemAnhHuongAsync(int chu, int? ma, int? mon, LichHocRequest? request, bool xoa, CancellationToken ct)
    {
        await using var phien = await dieuPhoi.MoAsync(chu, null, false, ct);
        var row = ma.HasValue ? await TimAsync(chu, ma.Value, ct) : null;
        var source = await MonAsync(chu, row?.MaMonHoc ?? mon ?? throw new HocTapException(400, "VALIDATION_ERROR", "Thiếu mã môn."), ct);
        AnhHuongLichResponse impact;
        if (xoa) { if (row is null) throw new HocTapException(400, "VALIDATION_ERROR", "Xóa cần mã lịch."); impact = new([], [], false); }
        else
        {
            if (request is null) throw new HocTapException(400, "VALIDATION_ERROR", "Thiếu nội dung lịch học.");
            KiemTra(request, source.HocKy, QuyTacThoiGian.MuiGio(phien.NguoiDung.MuiGio));
            (impact, _) = await TinhAnhHuongAsync(chu, Tao(source.Mon.Ma, request, ma ?? 0), source.HocKy, phien.NguoiDung.MuiGio, clock.GetUtcNow(), ct);
        }
        await phien.CommitAsync(false, ct);
        return new(phien.NguoiDung.PhienBanLapLich, impact);
    }

    private async Task<(AnhHuongLichResponse Impact, List<PhienTuHoc> Affected)> TinhAnhHuongAsync(int chu, LichHoc proposed, HocKy semester, string timezone, DateTimeOffset now, CancellationToken ct)
    {
        var zone = QuyTacThoiGian.MuiGio(timezone);
        var others = await calendar.TaiCoGioiHanAsync(store.TruyVan<LichHoc>().Where(x => x.MonHoc.HocKy.MaNguoiDung == chu && x.Ma != proposed.Ma)
            .Select(x => new DuLieuLichHoc(x, x.MonHoc, x.MonHoc.HocKy)), ct);
        var overlaps = others.Where(x => Trung(proposed, semester, x.Lich, x.HocKy)).Select(x => x.Lich.Ma).ToArray();
        var sessions = await QuyTacAnhHuongLich.LayPhienAsync(store, chu, now, ct);
        var affected = sessions.Where(x => QuyTacThoiGian.SinhLichHoc(proposed, semester, zone, x.BatDauDuKien, x.KetThucDuKien)
            .Any(k => QuyTacThoiGian.Giao(k, new(x.BatDauDuKien, x.KetThucDuKien)))).ToList();
        return (new(affected.Select(x => x.Ma).ToArray(), overlaps, await QuyTacAnhHuongLich.DangHocAsync(store, chu, affected, now, ct)), affected);
    }

    private static bool Trung(LichHoc a, HocKy ah, LichHoc b, HocKy bh)
    {
        if (a.ThuTrongTuan != b.ThuTrongTuan || a.GioBatDau >= b.GioKetThuc || b.GioBatDau >= a.GioKetThuc) return false;
        var tu = new[] { a.NgayApDungTu ?? ah.NgayBatDau, ah.NgayBatDau, b.NgayApDungTu ?? bh.NgayBatDau, bh.NgayBatDau }.Max();
        var den = new[] { a.NgayApDungDen ?? ah.NgayKetThuc, ah.NgayKetThuc, b.NgayApDungDen ?? bh.NgayKetThuc, bh.NgayKetThuc }.Min();
        return tu <= den && QuyTacThoiGian.CoNgay(tu, den, a.ThuTrongTuan);
    }

    private async Task<LichHoc> TimAsync(int chu, int ma, CancellationToken ct) =>
        await store.DauTienAsync(store.TruyVan<LichHoc>().Where(x => x.Ma == ma && x.MonHoc.HocKy.MaNguoiDung == chu), ct)
        ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy lịch học.");

    private async Task<(MonHoc Mon, HocKy HocKy)> MonAsync(int chu, int ma, CancellationToken ct)
    {
        var row = await store.DauTienAsync(store.TruyVan<MonHoc>().Where(x => x.Ma == ma && x.HocKy.MaNguoiDung == chu)
            .Select(x => new { Mon = x, HocKy = x.HocKy }), ct)
            ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy môn học.");
        return (row.Mon, row.HocKy);
    }

    private static void KiemTra(LichHocRequest r, HocKy semester, TimeZoneInfo zone)
    {
        if (r.ThuTrongTuan is < 1 or > 7 || r.GioKetThuc <= r.GioBatDau || r.PhongHoc?.Length > 200 || r.NgayApDungTu.HasValue != r.NgayApDungDen.HasValue)
            throw new HocTapException(400, "VALIDATION_ERROR", "Thứ, giờ, phòng hoặc cặp ngày áp dụng không hợp lệ.");
        var tu = r.NgayApDungTu ?? semester.NgayBatDau; var den = r.NgayApDungDen ?? semester.NgayKetThuc;
        if (tu > den || tu < semester.NgayBatDau || den > semester.NgayKetThuc || !QuyTacThoiGian.CoNgay(tu, den, r.ThuTrongTuan))
            throw new HocTapException(400, "VALIDATION_ERROR", "Khoảng ngày phải nằm trong học kỳ và có ít nhất một buổi đúng thứ.");
        // Kiểm tra từng lần lặp để không lưu mẫu chứa giờ DST không thể diễn giải.
        var localStart = QuyTacThoiGian.Utc(tu, TimeOnly.MinValue, zone);
        var localEnd = QuyTacThoiGian.Utc(den, TimeOnly.MaxValue, zone);
        _ = QuyTacThoiGian.SinhLichHoc(Tao(0, r, 0), semester, zone, localStart, localEnd);
    }

    private static LichHoc Tao(int mon, LichHocRequest r, int ma) { var e = new LichHoc { Ma = ma, MaMonHoc = mon }; Gan(e, r); return e; }
    private static void Gan(LichHoc e, LichHocRequest r)
    { e.ThuTrongTuan = r.ThuTrongTuan; e.GioBatDau = r.GioBatDau; e.GioKetThuc = r.GioKetThuc; e.PhongHoc = string.IsNullOrWhiteSpace(r.PhongHoc) ? null : r.PhongHoc.Trim(); e.NgayApDungTu = r.NgayApDungTu; e.NgayApDungDen = r.NgayApDungDen; }
    private static bool Giong(LichHoc e, LichHocRequest r) => e.ThuTrongTuan == r.ThuTrongTuan && e.GioBatDau == r.GioBatDau && e.GioKetThuc == r.GioKetThuc &&
        e.PhongHoc == (string.IsNullOrWhiteSpace(r.PhongHoc) ? null : r.PhongHoc.Trim()) && e.NgayApDungTu == r.NgayApDungTu && e.NgayApDungDen == r.NgayApDungDen;
    private static LichHocResponse Dto(LichHoc e, string ten) => new(e.Ma, e.MaMonHoc, ten, e.ThuTrongTuan, e.GioBatDau, e.GioKetThuc, e.PhongHoc, e.NgayApDungTu, e.NgayApDungDen, e.NgayTao, e.NgayCapNhat);
}
