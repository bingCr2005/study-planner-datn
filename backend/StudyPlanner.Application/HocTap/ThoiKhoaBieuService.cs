using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;

public sealed class ThoiKhoaBieuService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    internal const int GioiHanNguon = 10000;

    // Không mở transaction lồng: scheduler gọi hàm này trong snapshot/giao dịch do caller quản lý.
    public async Task<NguonThoiKhoaBieu> LayNguonAsync(int chu, DateTimeOffset tu, DateTimeOffset den, CancellationToken ct)
    {
        QuyTacThoiGian.KiemTraKhoang(tu, den);
        var user = await store.NguoiDungAsync(chu, false, ct);
        var zone = QuyTacThoiGian.MuiGio(user.MuiGio);
        var ngayTu = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(tu, zone).DateTime);
        var ngayDen = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(den, zone).DateTime);
        var lich = await TaiCoGioiHanAsync(store.TruyVan<LichHoc>().Where(x => x.MonHoc.HocKy.MaNguoiDung == chu &&
            (x.NgayApDungTu ?? x.MonHoc.HocKy.NgayBatDau) <= ngayDen && (x.NgayApDungDen ?? x.MonHoc.HocKy.NgayKetThuc) >= ngayTu)
            .Select(x => new DuLieuLichHoc(x, x.MonHoc, x.MonHoc.HocKy)), ct);
        var ranh = await TaiCoGioiHanAsync(store.TruyVan<KhungGioRanh>().Where(x => x.MaNguoiDung == chu && x.DangSuDung && x.NgayBatDau <= ngayDen && x.NgayKetThuc >= ngayTu), ct);
        var suKien = await TaiCoGioiHanAsync(store.TruyVan<NhiemVu>().Where(x => x.MaNguoiDung == chu &&
            x.BatDauSuKien != null && x.KetThucSuKien > tu && x.BatDauSuKien < den), ct);
        var phien = await TaiCoGioiHanAsync(store.TruyVan<PhienTuHoc>().Where(x => x.MaNguoiDung == chu &&
            x.KetThucDuKien > tu && x.BatDauDuKien < den), ct);
        return new(user,
            lich.SelectMany(x => QuyTacThoiGian.SinhLichHoc(x.Lich, x.HocKy, zone, tu, den)).ToArray(),
            QuyTacThoiGian.HopNhat(ranh.SelectMany(x => QuyTacThoiGian.SinhGioRanh(x, zone, tu, den))),
            suKien.Select(x => new KhoangThoiGian(x.BatDauSuKien!.Value, x.KetThucSuKien!.Value)).ToArray(), phien);
    }

    public async Task<HocTapResponse<ThoiKhoaBieuResponse>> LayAsync(int chu, DateTimeOffset tu, DateTimeOffset den,
        int? maHocKy, int? maMonHoc, bool hienHanCha, CancellationToken ct)
    {
        QuyTacThoiGian.KiemTraKhoang(tu, den);
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        await KiemTraPhamViAsync(chu, maHocKy, maMonHoc, ct);
        var user = session.NguoiDung;
        var zone = QuyTacThoiGian.MuiGio(user.MuiGio);
        var ngayTu = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(tu, zone).DateTime);
        var ngayDen = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(den, zone).DateTime);
        var events = new List<SuKienLichResponse>();
        var lichQuery = store.TruyVan<LichHoc>().Where(x => x.MonHoc.HocKy.MaNguoiDung == chu &&
            (x.NgayApDungTu ?? x.MonHoc.HocKy.NgayBatDau) <= ngayDen && (x.NgayApDungDen ?? x.MonHoc.HocKy.NgayKetThuc) >= ngayTu);
        if (maHocKy.HasValue) lichQuery = lichQuery.Where(x => x.MonHoc.MaHocKy == maHocKy);
        if (maMonHoc.HasValue) lichQuery = lichQuery.Where(x => x.MaMonHoc == maMonHoc);
        var lich = await TaiCoGioiHanAsync(lichQuery.Select(x => new DuLieuLichHoc(x, x.MonHoc, x.MonHoc.HocKy)), ct);
        foreach (var x in lich)
            foreach (var k in QuyTacThoiGian.SinhLichHoc(x.Lich, x.HocKy, zone, tu, den))
                events.Add(new($"class-{x.Lich.Ma}-{k.BatDau:yyyyMMddTHHmmssZ}", "LichHoc", x.Lich.Ma, x.Mon.Ma, x.HocKy.Ma,
                    x.Mon.Ten + (x.Lich.PhongHoc is null ? "" : " · " + x.Lich.PhongHoc), k.BatDau, k.KetThuc, x.Mon.MauSac, null, true));

        var tasks = store.TruyVan<NhiemVu>().Where(x => x.MaNguoiDung == chu);
        if (maHocKy.HasValue) tasks = tasks.Where(x => x.MaHocKy == maHocKy);
        if (maMonHoc.HasValue) tasks = tasks.Where(x => x.MaMonHoc == maMonHoc);
        var taskEvents = await TaiCoGioiHanAsync(tasks.Where(x => x.BatDauSuKien != null && x.KetThucSuKien > tu && x.BatDauSuKien < den)
            .Select(x => new { NhiemVu = x, MauSac = x.MonHoc == null ? null : x.MonHoc.MauSac }), ct);
        foreach (var x in taskEvents)
            events.Add(new($"event-{x.NhiemVu.Ma}", "SuKien", x.NhiemVu.Ma, x.NhiemVu.MaMonHoc, x.NhiemVu.MaHocKy,
                x.NhiemVu.TieuDe, x.NhiemVu.BatDauSuKien!.Value, x.NhiemVu.KetThucSuKien, x.MauSac, x.NhiemVu.TrangThai.ToString(), true));

        var deadlineQuery = tasks;
        var allTasks = store.TruyVan<NhiemVu>();
        if (!hienHanCha) deadlineQuery = deadlineQuery.Where(x => !allTasks.Any(con => con.MaNhiemVuCha == x.Ma));
        // Lọc hạn kế thừa trong SQL trước khi tải các cây cần hiển thị.
        var matchedDeadlines = QuyTacCay.LocHan(deadlineQuery, tu, den).OrderBy(x => x.Ma);
        var deadlines = await TaiCoGioiHanAsync(matchedDeadlines.Select(x => new { x.Ma, x.MaMonHoc, x.MaHocKy, x.TieuDe, x.TrangThai }), ct);
        // Hai SELECT cùng snapshot/thứ tự: dùng biểu thức SQL chung, không N+1 tải từng cây để tính hạn.
        var dates = await TaiCoGioiHanAsync(matchedDeadlines.Select(QuyTacCay.BieuThucHan), ct);
        for (var i = 0; i < deadlines.Count; i++)
        {
            var task = deadlines[i]; var han = dates[i];
            if (han is null) continue;
            events.Add(new($"deadline-{task.Ma}", "HanNhiemVu", task.Ma, task.MaMonHoc, task.MaHocKy,
                task.TieuDe, han.Value, null, null, task.TrangThai.ToString(), true));
        }

        var phienQuery = store.TruyVan<PhienTuHoc>().Where(x => x.MaNguoiDung == chu && x.TrangThai != TrangThaiPhienTuHoc.Cancelled &&
            x.KetThucDuKien > tu && x.BatDauDuKien < den);
        if (maHocKy.HasValue) phienQuery = phienQuery.Where(x => x.NhiemVu.MaHocKy == maHocKy);
        if (maMonHoc.HasValue) phienQuery = phienQuery.Where(x => x.NhiemVu.MaMonHoc == maMonHoc);
        var sessions = await TaiCoGioiHanAsync(phienQuery.Select(x => new { Phien = x, TieuDe = x.NhiemVu.TieuDe,
            x.NhiemVu.MaMonHoc, x.NhiemVu.MaHocKy, MauSac = x.NhiemVu.MonHoc == null ? null : x.NhiemVu.MonHoc.MauSac }), ct);
        foreach (var x in sessions)
            events.Add(new($"session-{x.Phien.Ma}", "PhienTuHoc", x.Phien.Ma, x.MaMonHoc, x.MaHocKy, x.TieuDe,
                x.Phien.BatDauDuKien, x.Phien.KetThucDuKien, x.MauSac, x.Phien.TrangThai.ToString(), x.Phien.TrangThai == TrangThaiPhienTuHoc.Planned));
        var ranh = await TaiCoGioiHanAsync(store.TruyVan<KhungGioRanh>().Where(x => x.MaNguoiDung == chu && x.DangSuDung && x.NgayBatDau <= ngayDen && x.NgayKetThuc >= ngayTu), ct);
        foreach (var x in ranh)
            foreach (var k in QuyTacThoiGian.SinhGioRanh(x, zone, tu, den))
                events.Add(new($"free-{x.Ma}-{k.BatDau:yyyyMMddTHHmmssZ}", "GioRanh", x.Ma, null, null, "Giờ rảnh", k.BatDau, k.KetThuc, null, null, true));
        if (events.Count > GioiHanNguon) throw new HocTapException(400, "CALENDAR_RESULT_LIMIT", "Lịch có quá nhiều sự kiện. Hãy thu hẹp khoảng ngày hoặc bộ lọc.");
        await session.CommitAsync(false, ct);
        return new(user.PhienBanLapLich, new(user.MuiGio, clock.GetUtcNow(), events.OrderBy(x => x.BatDau).ThenBy(x => x.Ma).ToArray()));
    }

    internal async Task<List<T>> TaiCoGioiHanAsync<T>(IQueryable<T> query, CancellationToken ct)
    {
        var rows = await store.DanhSachAsync(query.Take(GioiHanNguon + 1), ct);
        if (rows.Count > GioiHanNguon) throw new HocTapException(400, "CALENDAR_SOURCE_LIMIT", "Dữ liệu lịch vượt giới hạn 10.000 dòng. Hãy thu hẹp phạm vi hoặc dọn dữ liệu cũ.");
        return rows;
    }

    private async Task KiemTraPhamViAsync(int chu, int? hocKy, int? mon, CancellationToken ct)
    {
        if (hocKy.HasValue && await store.DemAsync(store.TruyVan<HocKy>().Where(x => x.Ma == hocKy && x.MaNguoiDung == chu), ct) == 0)
            throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy học kỳ.");
        if (mon.HasValue && await store.DemAsync(store.TruyVan<MonHoc>().Where(x => x.Ma == mon && x.HocKy.MaNguoiDung == chu && (!hocKy.HasValue || x.MaHocKy == hocKy)), ct) == 0)
            throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy môn học trong phạm vi.");
    }
}

internal sealed record DuLieuLichHoc(LichHoc Lich, MonHoc Mon, HocKy HocKy);
