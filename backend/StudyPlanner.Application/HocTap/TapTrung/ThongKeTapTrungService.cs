using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.HocTap;

public sealed class ThongKeTapTrungService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    public async Task<HocTapResponse<ThongKeTapTrungResponse>> DocAsync(int chu, DateOnly? ngay, int soNgay, CancellationToken ct)
    {
        if (soNgay is not (1 or 7 or 30)) throw new HocTapException(400, "VALIDATION_ERROR", "Chọn thống kê 1, 7 hoặc 30 ngày.");
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        var zone = QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime);
        var last = ngay ?? today;
        if (last > today || last.DayNumber < soNgay - 1) throw new HocTapException(400, "INVALID_DATE_RANGE", "Ngày kết thúc thống kê không được ở tương lai.");
        var first = last.AddDays(1 - soNgay);
        var from = QuyTacThoiGian.Utc(first, TimeOnly.MinValue, zone);
        var to = QuyTacThoiGian.Utc(last.AddDays(1), TimeOnly.MinValue, zone);
        var rows = await store.DanhSachAsync(store.TruyVan<PhienTapTrung>().Where(p => p.MaNguoiDung == chu &&
            p.TrangThai == "Completed" && p.KetThuc >= from && p.KetThuc < to)
            .OrderByDescending(p => p.BatDau).Take(10001), ct);
        if (rows.Count > 10000) throw new HocTapException(400, "QUERY_LIMIT", "Có quá nhiều bản ghi trong khoảng này. Hãy chọn khoảng ngắn hơn.");
        static int Pomo(PhienTapTrung p) => p.CheDo == "Pomodoro" ? (int)(p.SoMiliGiayDaHoc / (p.MucTieuGiay * 1000L)) : 0;
        var days = rows.ToLookup(p => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(p.KetThuc!.Value, zone).DateTime));
        var byDay = Enumerable.Range(0, soNgay).Select(i =>
        {
            var day = first.AddDays(i); var source = days[day].ToArray();
            return new TapTrungTheoNgay(day, source.Length, source.Sum(Pomo), source.Sum(p => p.SoMiliGiayDaHoc));
        }).ToArray();
        // Đổi tên nhiệm vụ giữa các phiên vẫn gom cùng một nhóm; bản ghi đã tách giữ tên được lưu lúc học.
        var byTask = rows.GroupBy(p => new { p.MaNhiemVu, TenDaBoLienKet = p.MaNhiemVu.HasValue ? "" : p.TieuDe })
            .Select(g => new TapTrungTheoNhiemVu(g.Key.MaNhiemVu, g.OrderByDescending(p => p.BatDau).First().TieuDe,
                g.Count(), g.Sum(Pomo), g.Sum(p => p.SoMiliGiayDaHoc)))
            .OrderByDescending(g => g.TongMiliGiay).ThenBy(g => g.TieuDe).ToArray();
        var summary = new TongHopTapTrung(rows.Count, rows.Sum(Pomo), rows.Sum(p => p.SoMiliGiayDaHoc));
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, new(first, last, session.NguoiDung.MuiGio, summary, byDay, byTask));
    }

    public async Task<HocTapResponse<TongHopTapTrung>> NhiemVuAsync(int chu, int ma, CancellationToken ct)
    {
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        if (await store.DemAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && t.Ma == ma && t.NgayXoa == null), ct) == 0)
            throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy nhiệm vụ của bạn.");
        var summary = await store.DauTienAsync(store.TruyVan<PhienTapTrung>().Where(p => p.MaNguoiDung == chu &&
            p.MaNhiemVu == ma && p.TrangThai == "Completed").GroupBy(p => 1).Select(g => new TongHopTapTrung(
                g.Count(), g.Sum(p => p.CheDo == "Pomodoro" ? (int)(p.SoMiliGiayDaHoc / (p.MucTieuGiay * 1000L)) : 0),
                g.Sum(p => p.SoMiliGiayDaHoc))), ct) ?? new(0, 0, 0);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, summary);
    }
}
