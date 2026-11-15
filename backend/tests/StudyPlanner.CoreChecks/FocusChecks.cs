using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
namespace StudyPlanner.CoreChecks;

public static class FocusChecks
{
    public static void Run()
    {
        var now = new DateTimeOffset(2026, 11, 12, 10, 0, 0, TimeSpan.Zero);
        var p = new PhienTapTrung { BatDau = now, BatDauDoan = now, MucTieuGiay = 1500 };
        void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL focus: " + name); }
        Check(QuyTacTapTrung.ThoiLuong(p, now.AddSeconds(30)) == 30000, "running elapsed uses server timestamps");
        Check(QuyTacTapTrung.ThoiLuong(p, now.AddHours(1)) == 1500000, "closed browser cannot overcount a countdown");
        Check(QuyTacTapTrung.HetGio(p) == now.AddMinutes(25), "countdown has an exact server deadline");
        p.SoMiliGiayDaHoc = 30000; p.BatDauDoan = null; p.TrangThai = "Paused";
        Check(QuyTacTapTrung.ThoiLuong(p, now.AddHours(1)) == 30000 && QuyTacTapTrung.HetGio(p) is null, "pause does not count break time");
        p.BatDauDoan = now.AddMinutes(5); p.TrangThai = "Running";
        Check(QuyTacTapTrung.ThoiLuong(p, now.AddMinutes(6)) == 90000, "resume adds only the new segment");
        Check(QuyTacTapTrung.HetGio(p) == now.AddMinutes(29.5), "resume countdown excludes paused time");
        p.CheDo = "Stopwatch";
        Check(QuyTacTapTrung.ThoiLuong(p, now.AddHours(1)) == 3330000 && QuyTacTapTrung.HetGio(p) is null, "stopwatch counts upward independently of the countdown target");
        foreach (var (mode, target) in new[] { ("Unknown", 1500), ("Pomodoro", 59), ("Pomodoro", 10801) })
        {
            try { QuyTacTapTrung.KiemTraCheDo(mode, target); throw new Exception("Expected invalid focus settings"); }
            catch (HocTapException e) when (e.Code == "VALIDATION_ERROR") { }
        }
        Console.WriteLine("PASS: focus countdown/stopwatch/pause/resume/expiry/settings rules");
    }
}
