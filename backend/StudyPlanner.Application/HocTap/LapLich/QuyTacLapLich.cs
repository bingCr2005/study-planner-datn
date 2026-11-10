namespace StudyPlanner.Application.HocTap;

/// <summary>Heuristic có kết quả xác định; không đọc/ghi DB hoặc tự sửa deadline.</summary>
public static class QuyTacLapLich
{
    public const int PhienBan = 1;
    public const int PhienToiDaPhut = 50;
    public const int PhienToiThieuPhut = 25;
    public const int NghiPhut = 10;
    public const int GioiHanPhien = 2000;

    public static KetQuaXepLich Xep(IEnumerable<CongViecLapLich> congViec,
        IEnumerable<KhoangThoiGian> gioRanh, DateTimeOffset now)
    {
        var earliest = TronPhutLen(now);
        var gaps = QuyTacThoiGian.HopNhat(gioRanh)
            .Where(g => g.KetThuc > earliest)
            .Select(g => new KhoangThoiGian(g.BatDau < earliest ? earliest : g.BatDau, g.KetThuc)).ToList();
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
                    var capacity = (int)Math.Max(0, Math.Floor((end - start).TotalMinutes));
                    var minutes = Math.Min(Math.Min(PhienToiDaPhut, remaining), capacity);
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
                remaining -= duration;
                // Dành khoảng nghỉ ở cả hai phía để cả việc sau có thể chèn vào khe trước phiên này.
                gaps = QuyTacThoiGian.Tru(gaps,
                    [new(selected.BatDau.AddMinutes(-NghiPhut), selected.KetThuc.AddMinutes(NghiPhut))]).ToList();
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
}
