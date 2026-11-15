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

    public async Task<HocTapResponse<IReadOnlyList<KhungGioRanhResponse>>> ThemNhieuAsync(int chu, NhieuKhungGioRanhRequest r, long revision, CancellationToken ct)
    {
        if (r.ThuTrongTuan is null || r.ThuTrongTuan.Count is < 1 or > 7 || r.ThuTrongTuan.Distinct().Count() != r.ThuTrongTuan.Count)
            throw Invalid("Chọn từ 1 đến 7 thứ khác nhau.");
        await using var session = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var zone = QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio);
        var requests = r.ThuTrongTuan.Order().Select(day => new KhungGioRanhRequest { ThuTrongTuan = day,
            GioBatDau = r.GioBatDau, GioKetThuc = r.GioKetThuc, NgayBatDau = r.NgayBatDau, NgayKetThuc = r.NgayKetThuc, DangSuDung = r.DangSuDung }).ToArray();
        // Kiểm tra đủ các thứ trước khi thêm; cả nhóm lưu chung một giao dịch và chỉ tăng phiên bản một lần.
        foreach (var request in requests) KiemTra(request, zone);
        var existing = await TaiAsync(chu, ct);
        if (existing.Count + requests.Length > 10000) throw Invalid("Có quá nhiều mẫu giờ rảnh; hãy thu gọn trước khi thêm.");
        if (requests.Any(request => existing.Any(row => Giong(row, request))))
            throw new HocTapException(409, "AVAILABILITY_DUPLICATE", "Có mẫu giờ rảnh đã tồn tại trong các thứ đã chọn. Hãy bỏ chọn mẫu trùng.");
        var now = clock.GetUtcNow();
        var rows = requests.Select(request => { var row = new KhungGioRanh { MaNguoiDung = chu, NgayTao = now, NgayCapNhat = now }; Gan(row, request); store.Them(row); return row; }).ToArray();
        await session.CommitAsync(true, ct);
        return new(session.NguoiDung.PhienBanLapLich, rows.Select(Dto).ToArray());
    }

    public async Task<HocTapResponse<IReadOnlyList<NgoaiLeGioRanhResponse>>> NgoaiLeAsync(int chu, int ma, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        await TimAsync(chu, ma, ct);
        var rows = await calendar.TaiCoGioiHanAsync(store.TruyVan<NgoaiLeGioRanh>().Where(e => e.MaNguoiDung == chu && e.MaKhungGioRanh == ma).OrderBy(e => e.Ngay), ct);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, rows.Select(e => new NgoaiLeGioRanhResponse(ma, e.Ngay, e.BoQua, e.GioBatDau, e.GioKetThuc)).ToArray());
    }

    public async Task<HocTapResponse<AnhHuongLichResponse>> DoiNgoaiLeAsync(int chu, int ma, DateOnly date,
        NgoaiLeGioRanhRequest r, long? revision, bool preview, bool delete, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, revision, !preview, ct);
        var template = await TimAsync(chu, ma, ct);
        var zone = QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio);
        if (date == default || date < template.NgayBatDau || date > template.NgayKetThuc ||
            ((int)date.DayOfWeek + 6) % 7 + 1 != template.ThuTrongTuan)
            throw Invalid("Ngày điều chỉnh phải là một lần xuất hiện trong mẫu giờ rảnh.");
        if (!delete && ((!r.BoQua && (!r.GioBatDau.HasValue || !r.GioKetThuc.HasValue || r.GioKetThuc <= r.GioBatDau)) ||
            (r.BoQua && (r.GioBatDau.HasValue || r.GioKetThuc.HasValue)))) throw Invalid("Chọn bỏ ngày này hoặc nhập đủ giờ bắt đầu/kết thúc hợp lệ.");
        var now = clock.GetUtcNow();
        var localToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        if (date < localToday) throw Invalid("Không chỉnh giờ rảnh của ngày đã qua.");
        if (!delete && !r.BoQua) { _ = QuyTacThoiGian.Utc(date, r.GioBatDau!.Value, zone); _ = QuyTacThoiGian.Utc(date, r.GioKetThuc!.Value, zone); }
        var all = await TaiAsync(chu, ct);
        var exceptions = await calendar.TaiCoGioiHanAsync(store.TruyVan<NgoaiLeGioRanh>().Where(e => e.MaNguoiDung == chu), ct);
        var old = exceptions.SingleOrDefault(e => e.MaKhungGioRanh == ma && e.Ngay == date);
        var changed = delete ? old is not null : old is null || old.BoQua != r.BoQua || old.GioBatDau != r.GioBatDau || old.GioKetThuc != r.GioKetThuc;
        // Dựng dữ liệu dự kiến riêng để bước xem ảnh hưởng không sửa đối tượng đang được theo dõi trong cơ sở dữ liệu.
        var next = exceptions.Where(e => e != old).ToList();
        if (!delete) next.Add(new NgoaiLeGioRanh { MaNguoiDung = chu, MaKhungGioRanh = ma, Ngay = date, BoQua = r.BoQua, GioBatDau = r.GioBatDau, GioKetThuc = r.GioKetThuc });
        var sessions = await QuyTacAnhHuongLich.LayPhienAsync(store, chu, now, ct);
        // So toàn bộ giờ rảnh sau khi gộp: một mẫu khác còn bao phủ thì phiên không cần bị hủy.
        var affected = sessions.Where(s => {
            var before = QuyTacGioRanh.Sinh(all, exceptions, zone, s.BatDauDuKien, s.KetThucDuKien);
            var after = QuyTacGioRanh.Sinh(all, next, zone, s.BatDauDuKien, s.KetThucDuKien);
            return before.Any(k => k.BatDau <= s.BatDauDuKien && k.KetThuc >= s.KetThucDuKien) &&
                !after.Any(k => k.BatDau <= s.BatDauDuKien && k.KetThuc >= s.KetThucDuKien);
        }).ToList();
        var impact = new AnhHuongLichResponse(affected.Select(s => s.Ma).ToArray(), [], await QuyTacAnhHuongLich.DangHocAsync(store, chu, affected, now, ct));
        if (!preview && changed)
        {
            QuyTacAnhHuongLich.ApDung(affected, impact, r.HuyPhienLienQuan, now);
            if (delete) { if (old is not null) store.Xoa(old); }
            else {
                var saved = old ?? new NgoaiLeGioRanh { MaNguoiDung = chu, MaKhungGioRanh = ma, Ngay = date, NgayTao = now };
                saved.BoQua = r.BoQua; saved.GioBatDau = r.GioBatDau; saved.GioKetThuc = r.GioKetThuc; saved.NgayCapNhat = now;
                if (old is null) store.Them(saved);
            }
        }
        await session.CommitAsync(!preview && changed, ct);
        return new(session.NguoiDung.PhienBanLapLich, impact);
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
            var exceptions = await calendar.TaiCoGioiHanAsync(store.TruyVan<NgoaiLeGioRanh>().Where(e => e.MaNguoiDung == chu && e.MaKhungGioRanh == ma), ct);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime);
            if (exceptions.Any(e => e.Ngay >= today && (e.Ngay < request.NgayBatDau || e.Ngay > request.NgayKetThuc || ((int)e.Ngay.DayOfWeek + 6) % 7 + 1 != request.ThuTrongTuan)))
                throw new HocTapException(409, "AVAILABILITY_EXCEPTION_CONFLICT", "Mẫu mới không còn bao gồm một số ngày điều chỉnh. Hãy khôi phục các ngày đó trước khi đổi thứ hoặc khoảng hiệu lực.");
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
        // Phiên cũ ngoài các khung đã lưu không bị hủy chỉ vì người dùng thêm một khung mới.
        if (!ma.HasValue) return (new([], [], false), []);
        var all = await TaiAsync(chu, ct);
        var exceptions = await calendar.TaiCoGioiHanAsync(store.TruyVan<NgoaiLeGioRanh>().Where(e => e.MaNguoiDung == chu), ct);
        var updated = all.Where(x => x.Ma != ma).ToList();
        if (proposed is not null) updated.Add(proposed);
        var sessions = await QuyTacAnhHuongLich.LayPhienAsync(store, chu, now, ct);
        var affected = sessions.Where(x =>
        {
            var k = new KhoangThoiGian(x.BatDauDuKien, x.KetThucDuKien);
            if (!QuyTacGioRanh.Sinh(all, exceptions, zone, k.BatDau, k.KetThuc).Any(b => b.BatDau <= k.BatDau && b.KetThuc >= k.KetThuc)) return false;
            var remaining = QuyTacGioRanh.Sinh(updated, exceptions, zone, k.BatDau, k.KetThuc);
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
    private static HocTapException Invalid(string message) => new(400, "VALIDATION_ERROR", message);
}
