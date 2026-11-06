using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
namespace StudyPlanner.CoreChecks;

public static class ThoiGianChecks
{
    public static void Run()
    {
        var day = new DateTimeOffset(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
        KhoangThoiGian K(int tu, int den) => new(day.AddMinutes(tu), day.AddMinutes(den));
        Assert(!QuyTacThoiGian.Giao(K(0, 50), K(50, 100)), "Half-open intervals may touch without collision");
        Assert(QuyTacThoiGian.Giao(K(0, 51), K(50, 100)), "A one-minute overlap is detected");
        var merged = QuyTacThoiGian.HopNhat([K(50, 100), K(0, 50), K(25, 75), K(120, 130)]);
        Assert(merged.Count == 2 && merged[0] == K(0, 100) && merged[1] == K(120, 130), "Duplicate/adjacent availability is merged before capacity calculation");
        var remaining = QuyTacThoiGian.Tru([K(0, 180)], [K(-10, 10), K(50, 70), K(65, 100), K(150, 200)]);
        Assert(remaining.SequenceEqual(new[] { K(10, 50), K(100, 150) }), "Busy union clips boundaries and preserves only usable gaps");
        Assert(QuyTacThoiGian.Tru([K(10, 20)], [K(0, 100)]).Count == 0, "A fully blocked window has no capacity");
        Assert(QuyTacThoiGian.HopNhat([K(10, 10)]).Count == 0, "Zero-length source is not counted as availability");

        var zone = QuyTacThoiGian.MuiGio("Asia/Ho_Chi_Minh");
        var semester = new HocKy { NgayBatDau = new(2026, 11, 1), NgayKetThuc = new(2026, 11, 30) };
        var session = new LichHoc { ThuTrongTuan = 1, GioBatDau = new(7, 0), GioKetThuc = new(9, 0) };
        var queryStart = QuyTacThoiGian.Utc(new(2026, 11, 1), new(0, 0), zone);
        var queryEnd = QuyTacThoiGian.Utc(new(2026, 12, 1), new(0, 0), zone);
        var occurrences = QuyTacThoiGian.SinhLichHoc(session, semester, zone, queryStart, queryEnd);
        Assert(occurrences.Count == 5, "Every Monday within inclusive semester end is generated once");
        Assert(occurrences[^1].BatDau == new DateTimeOffset(2026, 11, 30, 0, 0, 0, TimeSpan.Zero), "Local 07:00 is converted to UTC using account zone");
        Assert(QuyTacThoiGian.SinhLichHoc(session, semester, zone, occurrences[0].KetThuc, occurrences[1].BatDau).Count == 0,
            "Calendar excludes occurrences touching either query boundary only");
        session.NgayApDungTu = new(2026, 11, 10); session.NgayApDungDen = new(2026, 11, 23);
        Assert(QuyTacThoiGian.SinhLichHoc(session, semester, zone, queryStart, queryEnd).Count == 2, "Explicit recurrence dates intersect semester/query boundaries");
        var leap = new HocKy { NgayBatDau = new(2024, 2, 29), NgayKetThuc = new(2024, 2, 29) };
        session.ThuTrongTuan = 4; session.NgayApDungTu = null; session.NgayApDungDen = null;
        Assert(QuyTacThoiGian.SinhLichHoc(session, leap, zone, QuyTacThoiGian.Utc(new(2024, 2, 29), new(0, 0), zone),
            QuyTacThoiGian.Utc(new(2024, 3, 1), new(0, 0), zone)).Count == 1, "Leap day is a valid recurrence date");
        Assert(!QuyTacThoiGian.CoNgay(new(2026, 11, 4), new(2026, 11, 4), 1), "A single Wednesday cannot host a Monday recurrence");
        Assert(QuyTacThoiGian.CoNgay(new(2026, 11, 4), new(2026, 11, 4), 3), "Weekday numbering is Monday=1, Sunday=7");
        var free = new KhungGioRanh { ThuTrongTuan = 3, GioBatDau = new(19, 0), GioKetThuc = new(21, 0), NgayBatDau = new(2026, 11, 1), NgayKetThuc = new(2026, 11, 30), DangSuDung = false };
        Assert(QuyTacThoiGian.SinhGioRanh(free, zone, queryStart, queryEnd).Count == 0, "Disabled availability produces no slots");

        var dst = QuyTacThoiGian.MuiGio("America/New_York");
        Expect("LOCAL_TIME_AMBIGUOUS", () => QuyTacThoiGian.Utc(new(2026, 3, 8), new(2, 30), dst));
        Expect("LOCAL_TIME_AMBIGUOUS", () => QuyTacThoiGian.Utc(new(2026, 11, 1), new(1, 30), dst));
        Expect("INVALID_DATE_RANGE", () => QuyTacThoiGian.KiemTraKhoang(day, day.AddDays(94)));
        Console.WriteLine("PASS: recurrence, timezone, half-open intervals, availability union/subtraction and DST checks");
    }

    private static void Assert(bool passed, string name) { if (!passed) throw new Exception("ThoiGianChecks: " + name); }
    private static void Expect(string code, Action action)
    {
        try { action(); }
        catch (HocTapException e) when (e.Code == code) { return; }
        throw new Exception("ThoiGianChecks expected " + code);
    }
}
