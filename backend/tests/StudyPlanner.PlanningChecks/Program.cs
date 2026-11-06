using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.HocTap;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var count = 0;
void Check(bool pass, string message)
{
    if (!pass) throw new Exception("FAIL: " + message);
    count++; Console.WriteLine("PASS: " + message);
}
void Invalid(Action action, string message)
{
    try { action(); } catch (HocTapException) { Check(true, message); return; }
    throw new Exception("FAIL: " + message);
}
var now = new DateTimeOffset(2026, 11, 4, 18, 0, 0, TimeSpan.Zero);
var free = new KhoangThoiGian(now.AddHours(1), now.AddHours(3));
var job = new CongViecLapLich(1, "ERD", now.AddDays(1), MucDoUuTienNhiemVu.High, 100);
var result = QuyTacLapLich.Xep([job], [free], now);
Check(result.Phien.Count == 2, "100 minutes becomes two sessions");
Check(result.Phien.All(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes == 50), "Each session is at most 50 minutes");
Check(result.Phien[1].BatDauDuKien - result.Phien[0].KetThucDuKien == TimeSpan.FromMinutes(10), "Ten-minute break is kept");
Check(result.ChuaXep[1] == 0, "Full allocation has no missing minutes");
Check(result.Phien[0].BatDauDuKien == free.BatDau, "Earliest feasible slot is selected");

var partial = QuyTacLapLich.Xep([job], [new(free.BatDau, free.BatDau.AddHours(1))], now);
Check(partial.Phien.Count == 1 && partial.ChuaXep[1] == 50, "Insufficient availability returns partial allocation and exact shortage");
Check(partial.Phien.Sum(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes) == 50, "No allocation outside availability");

var small = QuyTacLapLich.Xep([job with { SoPhutCanXep = 10 }], [new(free.BatDau, free.BatDau.AddMinutes(10))], now);
Check(small.Phien.Count == 1 && small.ChuaXep[1] == 0, "Final ten-minute remainder is allowed");
var tooSmall = QuyTacLapLich.Xep([job], [new(free.BatDau, free.BatDau.AddMinutes(10))], now);
Check(tooSmall.Phien.Count == 0 && tooSmall.ChuaXep[1] == 100, "Ten-minute slot does not start a long task");

var deadline = QuyTacLapLich.Xep([job with { HanHieuLuc = free.BatDau.AddMinutes(30) }], [free], now);
Check(deadline.Phien.Count == 1 && deadline.Phien[0].KetThucDuKien == free.BatDau.AddMinutes(30), "Session is clipped at effective deadline");
Check(deadline.ChuaXep[1] == 70, "Deadline shortage is explicit");
var past = QuyTacLapLich.Xep([job with { HanHieuLuc = now.AddMinutes(-1) }], [free], now);
Check(past.Phien.Count == 0, "No placement after an elapsed deadline");

var merged = QuyTacLapLich.Xep([job], [free, free, new(free.BatDau.AddMinutes(20), free.KetThuc)], now);
Check(merged.Phien.SequenceEqual(result.Phien), "Overlapping availability does not multiply capacity");
var busy = new KhoangThoiGian(free.BatDau.AddMinutes(30), free.BatDau.AddMinutes(90));
var gaps = QuyTacThoiGian.Tru([free], [busy]);
var around = QuyTacLapLich.Xep([job], gaps, now);
Check(around.Phien.All(p => !QuyTacThoiGian.Giao(new(p.BatDauDuKien, p.KetThucDuKien), busy)), "Busy interval is never crossed");
Check(around.ChuaXep[1] == 40, "Fragmented thirty-minute windows allocate only sixty minutes");

var orderedJobs = new[] {
    new CongViecLapLich(4, "No deadline", null, MucDoUuTienNhiemVu.High, 25),
    new CongViecLapLich(3, "Medium", now.AddDays(1), MucDoUuTienNhiemVu.Medium, 25),
    new CongViecLapLich(2, "High", now.AddDays(1), MucDoUuTienNhiemVu.High, 25),
    new CongViecLapLich(1, "Soon", now.AddHours(5), MucDoUuTienNhiemVu.Low, 25)
};
var ordered = QuyTacLapLich.Xep(orderedJobs, [new(now.AddHours(1), now.AddHours(6))], now);
Check(ordered.Phien.Select(p => p.MaNhiemVu).SequenceEqual([1, 2, 3, 4]), "Ordering is deadline, priority, remaining work, ID");
Check(ordered.Phien.Zip(ordered.Phien.Skip(1)).All(p => p.Second.BatDauDuKien >= p.First.KetThucDuKien.AddMinutes(10)), "Breaks remain valid across different tasks");
var stable = QuyTacLapLich.Xep(orderedJobs.Reverse(), [new(now.AddHours(1), now.AddHours(6))], now);
Check(ordered.Phien.SequenceEqual(stable.Phien), "Input enumeration order does not change schedule");

var tinyThenLong = QuyTacLapLich.Xep([
    new(1, "Long", now.AddDays(1), MucDoUuTienNhiemVu.High, 100),
    new(2, "Tiny", now.AddDays(2), MucDoUuTienNhiemVu.Low, 10)],
    [new(now.AddHours(1), now.AddHours(1).AddMinutes(10))], now);
Check(tinyThenLong.Phien.Count == 1 && tinyThenLong.Phien[0].MaNhiemVu == 2, "Unplaceable first task does not block smaller later work");
Check(QuyTacLapLich.TronPhutLen(now.AddSeconds(1)) == now.AddMinutes(1), "Start rounds forward, never into the past");

var signer = new HmacPreviewLapLichToken(Enumerable.Range(1, 64).Select(x => (byte)x).ToArray());
var payload = new NoiDungPreviewLapLich(1, 9, 1, now, now.AddMinutes(10),
    result.Phien.Select(p => new ChonPhienTuHocRequest(p.MaNhiemVu, p.BatDauDuKien, p.KetThucDuKien)).ToList());
var token = signer.Tao(payload);
var read = signer.Doc(token);
Check(read.MaNguoiDung == 1 && read.PhienBanNguon == 9 && read.Phien.Count == 2, "Signed token round-trips owner, revision and slots");
Invalid(() => signer.Doc("invalid"), "Malformed token is rejected");
var parts = token.Split('.'); parts[0] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{}"));
Invalid(() => signer.Doc(string.Join('.', parts)), "Edited payload cannot retain valid signature");
Invalid(() => new HmacPreviewLapLichToken(new byte[64]).Doc(token), "Token signed with a different key is rejected");

using (var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
           .UseNpgsql("Host=localhost;Database=planning_metadata;Username=unused").Options))
{
    var sourceQuery = db.DanhSachNhiemVu.Where(t => t.MaNguoiDung == 1);
    var filteredSql = QuyTacCay.LocHan(sourceQuery, now, now.AddDays(7)).OrderBy(QuyTacCay.BieuThucHan).Take(20).ToQueryString();
    Check(filteredSql.Contains("COALESCE") && filteredSql.Contains("LEFT JOIN") && filteredSql.Contains("LIMIT"),
        "Effective inherited deadline filtering and paging translate to PostgreSQL SQL");
    var undatedSql = QuyTacCay.LocHan(sourceQuery, null, null, true).ToQueryString();
    Check(undatedSql.Contains("IS NULL"), "Undated filter translates without evaluating a null parent in memory");
    var leafSql = sourceQuery.Where(t => !t.DanhSachNhiemVuCon.Any()).ToQueryString();
    Check(leafSql.Contains("NOT EXISTS"), "Leaf predicate remains a database query");
    var index = db.Model.FindEntityType(typeof(PhienTuHoc))!.GetIndexes().Single(i => i.GetDatabaseName() == "UX_phien_tu_hoc_dang_hoc");
    Check(index.IsUnique && index.GetFilter() == "trang_thai = 'InProgress'", "Model includes unique one-active-session guard");
}

// Generated cases cover interval/deadline/remaining invariants without mirroring exact placement.
var random = new Random(407);
for (var trial = 0; trial < 60; trial++)
{
    var jobs = Enumerable.Range(1, 12).Select(i => new CongViecLapLich(i, "Task " + i,
        i % 4 == 0 ? null : now.AddHours(3 + i % 5), (MucDoUuTienNhiemVu)(i % 3), random.Next(1, 200))).ToArray();
    var ranges = new[] {new KhoangThoiGian(now.AddHours(1), now.AddHours(2)), new(now.AddHours(3), now.AddHours(8))};
    var generated = QuyTacLapLich.Xep(jobs, ranges, now);
    Check(generated.Phien.All(p => ranges.Any(r => r.BatDau <= p.BatDauDuKien && r.KetThuc >= p.KetThucDuKien) &&
        (!jobs[p.MaNhiemVu - 1].HanHieuLuc.HasValue || p.KetThucDuKien <= jobs[p.MaNhiemVu - 1].HanHieuLuc)), "Generated schedule stays in availability and before deadlines " + trial);
    Check(jobs.All(j => generated.Phien.Where(p => p.MaNhiemVu == j.MaNhiemVu).Sum(p => (p.KetThucDuKien - p.BatDauDuKien).TotalMinutes) + generated.ChuaXep[j.MaNhiemVu] == j.SoPhutCanXep), "Generated allocation conserves remaining workload " + trial);
    Check(generated.Phien.Zip(generated.Phien.Skip(1)).All(p => p.Second.BatDauDuKien >= p.First.KetThucDuKien.AddMinutes(10)), "Generated schedule preserves breaks " + trial);
}
Console.WriteLine($"Planning checks passed: {count} assertions. No database connection was opened.");
