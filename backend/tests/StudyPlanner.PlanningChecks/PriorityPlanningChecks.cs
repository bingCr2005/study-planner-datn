using System.Diagnostics;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.UuTien;
using StudyPlanner.Domain.Enums;

internal static class PriorityPlanningChecks
{
    public static void Run(Action<bool, string> check, DateTimeOffset now)
    {
        var config = QuyTacCauHinhUuTien.MacDinh();
        var a = new CongViecLapLich(1, "A cao còn 3 ngày", now.AddDays(3), MucDoUuTienNhiemVu.High, 25);
        var b = new CongViecLapLich(2, "B thấp còn 2 ngày", now.AddDays(2), MucDoUuTienNhiemVu.Low, 25);
        KhoangThoiGian early = new(now.AddDays(1), now.AddDays(1).AddMinutes(25));
        KhoangThoiGian late = new(now.AddDays(2).AddHours(1), now.AddDays(2).AddHours(1).AddMinutes(25));
        var one = QuyTacLapLich.Xep([b, a], [early], now);
        check(one.Phien.Single().MaNhiemVu == a.MaNhiemVu, "K2 one scarce slot preserves Admin importance priority");
        var two = QuyTacLapLich.Xep([a, b], [early, late], now);
        check(two.Phien.Select(p => p.MaNhiemVu).SequenceEqual([2, 1]) && two.ChuaXep.Values.All(x => x == 0),
            "K2 B takes only before-deadline slot and A takes later slot, both complete");
        check(two.Phien.All(p => p.KetThucDuKien <= (p.MaNhiemVu == 1 ? a.HanHieuLuc : b.HanHieuLuc)), "Feasibility pass never changes deadlines");
        check(two.Phien.All(p => p.LyDo.Contains("Điều chỉnh thứ tự")), "Feasibility improvement is explained");
        var deadline = new CauHinhUuTien { PhienBanSchema = 1, TieuChi = ["Deadline", "Importance", "InProgress"] };
        check(QuyTacLapLich.Xep([a, b], [early], now, uuTien: deadline).Phien.Single().MaNhiemVu == 2,
            "Changed Admin rule immediately changes scarce-slot ranking");
        var tie = a with { MaNhiemVu = 3, TrangThai = TrangThaiNhiemVu.InProgress };
        check(QuyTacUuTienNhiemVu.SapXep([a, tie], now, config).First().MaNhiemVu == 3, "InProgress breaks importance and deadline ties");
        check(QuyTacUuTienNhiemVu.SapXep([a with { MaNhiemVu = 9 }, a], now, config).First().MaNhiemVu == 1, "Task ID is the final stable tie breaker");
        check(QuyTacLapLich.Xep([a with { SoPhutCanXep = 50 }, a with { MaNhiemVu = 2 }], [early], now).Phien.Single().MaNhiemVu == 1,
            "Shorter workload does not override the configured criteria and ID");
        check(QuyTacUuTienNhiemVu.SapXep([a, tie], now, new() { PhienBanSchema = 1, TieuChi = ["Deadline"] }).First().MaNhiemVu == 1,
            "Disabled InProgress criterion cannot override ID");
        foreach (var (hours, expected) in new (double, int)[] { (-0.001, 0), (0, 1), (24, 1), (24.001, 2), (72, 2), (72.001, 3), (168, 3), (168.001, 4) })
            check(QuyTacUuTienNhiemVu.Nhom(now.AddHours(hours), now) == expected, "Urgency boundary " + hours);
        check(QuyTacUuTienNhiemVu.Nhom(null, now) == 5, "Undated remains K5");
        var all = new[] { a with { HanHieuLuc = null }, a with { MaNhiemVu = 2, HanHieuLuc = now.AddDays(9) },
            a with { MaNhiemVu = 3, HanHieuLuc = now.AddDays(5) }, a with { MaNhiemVu = 4 },
            a with { MaNhiemVu = 5, HanHieuLuc = now.AddHours(1), MucDoUuTien = MucDoUuTienNhiemVu.Low },
            a with { MaNhiemVu = 6, HanHieuLuc = now.AddMinutes(-1) } };
        check(QuyTacUuTienNhiemVu.SapXep(all, now, config).Select(j => j.MaNhiemVu).SequenceEqual([6, 5, 4, 3, 2, 1]), "All six groups dominate importance");
        var capped = QuyTacLapLich.Xep([a, b], [early, late], now, new(25, 5, 25));
        check(capped.ChuaXep.Values.All(x => x == 0), "Fallback respects daily cap across two days");
        check(QuyTacLapLich.Xep([a, b], [], now).Phien.Count == 0, "Missing free time produces no sessions");
        // B cannot win by sacrificing A when only B has a deadline-fitting slot.
        var scarce = QuyTacLapLich.Xep([a with { SoPhutCanXep = 50 }, b], [early, late], now);
        check(scarce.ChuaXep[1] == 0 && scarce.ChuaXep[2] == 25, "Fallback rejects a completion gain that takes minutes from another task");
        var timer = Stopwatch.StartNew();
        var large = Enumerable.Range(1, 300).Select(i => new CongViecLapLich(i, "Fixture", now.AddDays(1 + i % 20),
            (MucDoUuTienNhiemVu)(i % 3), 50)).ToArray();
        var ranges = Enumerable.Range(1, 21).Select(i => new KhoangThoiGian(now.AddDays(i), now.AddDays(i).AddHours(4))).ToArray();
        var generated = QuyTacLapLich.Xep(large, ranges, now);
        check(timer.Elapsed < TimeSpan.FromSeconds(10) && generated.ChuaXep.Count == 300, "Bounded two-pass planner handles 300-task fixture under ten seconds");
        check(large.All(j => generated.Phien.Where(p => p.MaNhiemVu == j.MaNhiemVu).Sum(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes) + generated.ChuaXep[j.MaNhiemVu] == j.SoPhutCanXep), "Large fixture conserves each task workload");
    }
}
