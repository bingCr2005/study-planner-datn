namespace StudyPlanner.Application.HocTap;

/// <summary>Xếp tham lam cho kết quả xác định; không đọc/ghi cơ sở dữ liệu hoặc tự sửa hạn hoàn thành.</summary>
public static class QuyTacLapLich
{
    public const int PhienBan = 2;
    public const int PhienToiDaPhut = 50;
    public const int PhienToiThieuPhut = 15;
    public const int NghiPhut = 5;
    public const int GioiHanPhien = 2000;

    public static KetQuaXepLich Xep(IEnumerable<CongViecLapLich> congViec,
        IEnumerable<KhoangThoiGian> gioRanh, DateTimeOffset now, CauHinhHocResponse? cauHinh = null,
        TimeZoneInfo? muiGio = null, IReadOnlyDictionary<DateOnly, double>? phutDaDat = null)
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
        foreach (var job in congViec.OrderBy(x => x.HanHieuLuc ?? DateTimeOffset.MaxValue)
                     .ThenByDescending(x => x.MucDoUuTien).ThenBy(x => x.SoPhutCanXep).ThenBy(x => x.MaNhiemVu))
        {
            var remaining = job.SoPhutCanXep;
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
                    // Chỉ cho phép phiên ngắn hơn tối thiểu khi nó xếp hết phần cuối còn lại của nhiệm vụ.
                    if (minutes <= 0 || minutes < PhienToiThieuPhut && minutes != remaining) continue;
                    selected = new(start, start.AddMinutes(minutes)); duration = minutes; break;
                }
                if (selected is null) break;
                if (sessions.Count == GioiHanPhien)
                    throw new HocTapException(400, "PLANNING_LIMIT", "Gợi ý vượt 2.000 phiên; hãy thu hẹp phạm vi.");
                var reason = job.HanHieuLuc.HasValue
                    ? "Ưu tiên hạn gần; tiếp theo mức ưu tiên và phần thời lượng cần xếp."
                    : "Việc không hạn được xếp sau các việc có hạn, trong giờ rảnh còn lại.";
                sessions.Add(new(job.MaNhiemVu, selected.BatDau, selected.KetThuc, job.TieuDe, reason));
                var selectedDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(selected.BatDau, zone).DateTime);
                dayMinutes[selectedDay] = dayMinutes.GetValueOrDefault(selectedDay) + duration;
                remaining -= duration;
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
