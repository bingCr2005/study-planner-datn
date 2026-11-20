using StudyPlanner.Application.UuTien;

namespace StudyPlanner.Application.HocTap;

/// <summary>Xếp tham lam cho kết quả xác định; không đọc/ghi cơ sở dữ liệu hoặc tự sửa hạn hoàn thành.</summary>
public static class QuyTacLapLich
{
    public const int PhienBan = 4;
    public const int PhienToiDaPhut = 50;
    public const int PhienToiThieuPhut = 15;
    public const int NghiPhut = 5;
    public const int GioiHanPhien = 2000;

    public static KetQuaXepLich Xep(IEnumerable<CongViecLapLich> congViec,
        IEnumerable<KhoangThoiGian> gioRanh, DateTimeOffset now, CauHinhHocResponse? cauHinh = null,
        TimeZoneInfo? muiGio = null, IReadOnlyDictionary<DateOnly, double>? phutDaDat = null, CauHinhUuTien? uuTien = null)
    {
        var jobs = congViec.ToArray(); var free = gioRanh.ToArray();
        var config = uuTien ?? QuyTacCauHinhUuTien.MacDinh();
        var baseline = XepTheoThuTu(QuyTacUuTienNhiemVu.SapXep(jobs, now, config), free, now, cauHinh, muiGio, phutDaDat, config);
        if (!jobs.Any(j => j.HanHieuLuc.HasValue && baseline.ChuaXep[j.MaNhiemVu] > 0)) return baseline;
        // Thử thêm một thứ tự trong từng nhóm, dùng lại bộ phân bổ và mọi ràng buộc.
        var alternative = XepTheoThuTu(QuyTacUuTienNhiemVu.SapXep(jobs, now, config, true), free, now, cauHinh, muiGio, phutDaDat, config);
        // Chỉ nhận nếu không việc nào mất phút và ít nhất một việc được xếp đủ thêm.
        if (jobs.All(j => alternative.ChuaXep[j.MaNhiemVu] <= baseline.ChuaXep[j.MaNhiemVu]) &&
            jobs.Any(j => baseline.ChuaXep[j.MaNhiemVu] > 0 && alternative.ChuaXep[j.MaNhiemVu] == 0))
            return alternative with { Phien = alternative.Phien.Select(p => p with
                { LyDo = p.LyDo + " · Điều chỉnh thứ tự trong nhóm để xếp đủ trước hạn" }).ToList() };
        return baseline;
    }

    private static KetQuaXepLich XepTheoThuTu(IEnumerable<CongViecLapLich> congViec,
        IEnumerable<KhoangThoiGian> gioRanh, DateTimeOffset now, CauHinhHocResponse? cauHinh,
        TimeZoneInfo? muiGio, IReadOnlyDictionary<DateOnly, double>? phutDaDat, CauHinhUuTien uuTien)
    {
        var settings = cauHinh ?? CauHinhHocResponse.MacDinh; settings.KiemTra();
        var zone = muiGio ?? TimeZoneInfo.Utc;
        var dayMinutes = phutDaDat?.ToDictionary(row => row.Key, row => row.Value) ?? [];
        var earliest = TronPhutLen(now);
        // Tách theo nửa đêm địa phương trước khi tính hạn mức để phiên qua ngày không dùng nhầm ngân sách.
        var gaps = QuyTacThoiGian.HopNhat(gioRanh)
            .Where(g => g.KetThuc > earliest)
            .Select(g => new KhoangThoiGian(g.BatDau < earliest ? earliest : g.BatDau, g.KetThuc))
            .SelectMany(g => ChiaNgay(g, zone)).ToList();
        var sessions = new List<DeXuatPhienTuHoc>();
        var missing = new Dictionary<int, int>();
        foreach (var job in congViec)
        {
            var remaining = job.SoPhutCanXep;
            var lastEnd = earliest;
            while (remaining > 0)
            {
                KhoangThoiGian? selected = null;
                var duration = 0;
                foreach (var gap in gaps.OrderBy(g => g.BatDau))
                {
                    var start = TronPhutLen(gap.BatDau);
                    var end = job.HanHieuLuc is DateTimeOffset due && due < gap.KetThuc ? due : gap.KetThuc;
                    var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(start, zone).DateTime);
                    var midnight = QuyTacThoiGian.Utc(day.AddDays(1), TimeOnly.MinValue, zone);
                    if (end > midnight) end = midnight;
                    var capacity = (int)Math.Max(0, Math.Floor((end - start).TotalMinutes));
                    var dayCapacity = (int)Math.Max(0, Math.Floor(settings.GioiHanHocMoiNgayPhut - dayMinutes.GetValueOrDefault(day)));
                    var minutes = Math.Min(Math.Min(settings.PhienHocPhut, remaining), Math.Min(capacity, dayCapacity));
                    // Phần cuối phải nằm sau các phiên của cùng nhiệm vụ theo thứ tự thời gian.
                    if (minutes <= 0 || minutes < PhienToiThieuPhut && (minutes != remaining || start < lastEnd)) continue;
                    selected = new(start, start.AddMinutes(minutes)); duration = minutes; break;
                }
                if (selected is null) break;
                if (sessions.Count == GioiHanPhien)
                    throw new HocTapException(400, "PLANNING_LIMIT", "Gợi ý vượt 2.000 phiên; hãy thu hẹp phạm vi.");
                var reason = QuyTacUuTienNhiemVu.LyDo(job, now, uuTien);
                sessions.Add(new(job.MaNhiemVu, selected.BatDau, selected.KetThuc, job.TieuDe, reason));
                var selectedDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(selected.BatDau, zone).DateTime);
                dayMinutes[selectedDay] = dayMinutes.GetValueOrDefault(selectedDay) + duration;
                remaining -= duration;
                if (selected.KetThuc > lastEnd) lastEnd = selected.KetThuc;
                // Dành khoảng nghỉ ở cả hai phía để cả việc sau có thể chèn vào khe trước phiên này.
                gaps = QuyTacThoiGian.Tru(gaps,
                    [new(selected.BatDau.AddMinutes(-settings.NghiGiuaPhienPhut), selected.KetThuc.AddMinutes(settings.NghiGiuaPhienPhut))])
                    .SelectMany(gap => ChiaNgay(gap, zone)).ToList();
            }
            missing[job.MaNhiemVu] = remaining;
        }
        return new(sessions.OrderBy(s => s.BatDauDuKien).ThenBy(s => s.MaNhiemVu).ToList(), missing);
    }

    public static DateTimeOffset TronPhutLen(DateTimeOffset time)
    {
        var ticks = time.UtcTicks;
        var rounded = ticks / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute;
        return new DateTimeOffset(rounded == ticks ? rounded : rounded + TimeSpan.TicksPerMinute, TimeSpan.Zero);
    }

    private static IEnumerable<KhoangThoiGian> ChiaNgay(KhoangThoiGian range, TimeZoneInfo zone)
    {
        var cursor = range.BatDau;
        while (cursor < range.KetThuc)
        {
            var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(cursor, zone).DateTime);
            var end = QuyTacThoiGian.Utc(day.AddDays(1), TimeOnly.MinValue, zone);
            if (end > range.KetThuc) end = range.KetThuc;
            yield return new(cursor, end); cursor = end;
        }
    }
}
