using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.HocTap;

public static class QuyTacTapTrung
{
    public static bool DangMo(PhienTapTrung p) => p.TrangThai is "Running" or "Paused";
    public static long ThoiLuong(PhienTapTrung p, DateTimeOffset now)
    {
        var milliseconds = p.SoMiliGiayDaHoc + (p.BatDauDoan is DateTimeOffset start
            ? Math.Max(0, (now - start).Ticks / TimeSpan.TicksPerMillisecond) : 0);
        return p.CheDo == "Pomodoro" ? Math.Min(milliseconds, p.MucTieuGiay * 1000L) : milliseconds;
    }
    public static DateTimeOffset? HetGio(PhienTapTrung p) => p.CheDo == "Pomodoro" && p.BatDauDoan.HasValue
        ? p.BatDauDoan.Value.AddMilliseconds(Math.Max(0, p.MucTieuGiay * 1000L - p.SoMiliGiayDaHoc)) : null;
    public static void KiemTraCheDo(string mode, int target)
    {
        if (mode is not ("Pomodoro" or "Stopwatch") || target is < 60 or > 10800)
            throw new HocTapException(400, "VALIDATION_ERROR", "Chọn Pomodoro hoặc đếm lên; thời lượng Pomodoro từ 1–180 phút.");
    }
}
