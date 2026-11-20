using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.CoreChecks;

public static class PhienTuHocChecks
{
    public static async Task RunAsync()
    {
        var count = 0;
        void Check(bool condition, string label)
        {
            if (!condition) throw new Exception("FAIL session: " + label);
            count++;
        }
        async Task Reject(string code, Func<Task> action)
        {
            try { await action(); }
            catch (HocTapException e) when (e.Code == code) { count++; return; }
            throw new Exception("Expected session rejection: " + code);
        }

        var f = new Fixture();
        var actualStart = f.Clock.GetUtcNow();
        var plannedStart = f.Session.BatDauDuKien;
        var plannedEnd = f.Session.KetThucDuKien;
        var started = await f.Change("start");
        Check(started.DuLieu.TrangThai == TrangThaiPhienTuHoc.InProgress, "A future Planned session can start immediately");
        Check(started.DuLieu.BatDauBamGio == actualStart, "The stopwatch uses actual server time when starting early");
        Check(started.DuLieu.BatDauDuKien == plannedStart && started.DuLieu.KetThucDuKien == plannedEnd,
            "Starting early preserves the original planned interval");
        Check(started.PhienBan == 6 && f.Store.TruyVan<KhoangThoiGianHoc>().Count() == 1, "Start saves one segment and advances the revision");
        f.Clock.Advance(TimeSpan.FromMinutes(5));
        var repeated = await f.Change("start");
        Check(repeated.PhienBan == started.PhienBan && repeated.DuLieu.BatDauBamGio == actualStart &&
            f.Store.TruyVan<KhoangThoiGianHoc>().Count() == 1, "Repeated start preserves the stopwatch, segment and revision");
        var restored = await f.Service.ChiTietAsync(1, 1, default);
        Check(restored.DuLieu.BatDauBamGio == actualStart, "Reopening the session restores the running stopwatch");
        f.Clock.Advance(TimeSpan.FromMinutes(2));
        var finished = await f.Change("finish");
        Check(finished.DuLieu.TrangThai == TrangThaiPhienTuHoc.Completed && finished.DuLieu.BatDauBamGio is null &&
            finished.DuLieu.SoGiayDaHoc == 420, "Finishing before the planned start records seven actual minutes");
        Check(f.Task.TienDo == 25 && f.Task.TrangThai == TrangThaiNhiemVu.InProgress && f.Task.ThoiLuongConLaiPhut == 75,
            "Elapsed time does not automatically change task progress or the remaining estimate");
        f.Clock.Advance(TimeSpan.FromMinutes(1));
        var repeatedFinish = await f.Change("finish");
        Check(repeatedFinish.PhienBan == finished.PhienBan && repeatedFinish.DuLieu.SoGiayDaHoc == 420,
            "Repeated finish preserves the recorded duration and revision");
        await Reject("SESSION_STATE_CONFLICT", () => f.Change("start"));

        var atStart = new Fixture();
        atStart.Clock.Set(atStart.Session.BatDauDuKien);
        Check((await atStart.Change("start")).DuLieu.TrangThai == TrangThaiPhienTuHoc.InProgress, "Starting at the planned start still works");
        var late = new Fixture();
        late.Clock.Set(late.Session.KetThucDuKien.AddSeconds(-1));
        Check((await late.Change("start")).DuLieu.TrangThai == TrangThaiPhienTuHoc.InProgress, "Starting before the end still works");
        var ended = new Fixture();
        ended.Clock.Set(ended.Session.KetThucDuKien);
        await Reject("SESSION_STATE_CONFLICT", () => ended.Change("start"));
        var manualFinish = await ended.Change("finish");
        Check(manualFinish.DuLieu.SoGiayDaHoc is null && ended.Task.TienDo == 25,
            "Confirming an elapsed session without timing invents neither duration nor task progress");

        foreach (var state in new[] { TrangThaiPhienTuHoc.Cancelled, TrangThaiPhienTuHoc.Missed })
        {
            var closed = new Fixture(); closed.Session.TrangThai = state;
            await Reject("SESSION_STATE_CONFLICT", () => closed.Change("start"));
        }
        var otherRunning = new Fixture();
        otherRunning.Store.Them(new PhienTuHoc { Ma = 2, MaNguoiDung = 1, TrangThai = TrangThaiPhienTuHoc.InProgress });
        await Reject("SESSION_STATE_CONFLICT", () => otherRunning.Change("start"));
        Check(!otherRunning.Store.TruyVan<KhoangThoiGianHoc>().Any(), "A second running session cannot open another stopwatch");
        var otherTimer = new Fixture();
        otherTimer.Store.Them(new KhoangThoiGianHoc { MaNguoiDung = 1, MaNhiemVu = 2, BatDauThucTe = actualStart });
        await Reject("SESSION_STATE_CONFLICT", () => otherTimer.Change("start"));
        var completedTask = new Fixture();
        completedTask.Task.TrangThai = TrangThaiNhiemVu.Completed; completedTask.Task.TienDo = 100;
        await Reject("SESSION_STATE_CONFLICT", () => completedTask.Change("start"));
        var cancelled = new Fixture();
        await cancelled.Change("cancel");
        Check(cancelled.Session.TrangThai == TrangThaiPhienTuHoc.Cancelled, "Cancelling a future Planned session still works");
        var stale = new Fixture();
        await Reject("DATA_VERSION_CONFLICT", () => stale.Service.DoiTrangThaiAsync(1, 1, 4, "start", default));
        var foreign = new Fixture(); foreign.Session.MaNguoiDung = 2;
        await Reject("RESOURCE_NOT_FOUND", () => foreign.Change("start"));
        Console.WriteLine($"PASS: {count} session/early-start/stopwatch/progress assertions (in-memory store; no PostgreSQL)");
    }

    private sealed class Fixture
    {
        public KiemThuClock Clock { get; } = new(new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero));
        public MemoryStore Store { get; } = new();
        public NhiemVu Task { get; } = new() { Ma = 1, MaNguoiDung = 1, TieuDe = "B1", TienDo = 25,
            TrangThai = TrangThaiNhiemVu.InProgress, ThoiLuongDuKienPhut = 100 };
        public PhienTuHoc Session { get; }
        public PhienTuHocService Service { get; }
        public Fixture()
        {
            Session = new() { Ma = 1, MaNguoiDung = 1, MaNhiemVu = 1, TrangThai = TrangThaiPhienTuHoc.Planned,
                BatDauDuKien = Clock.GetUtcNow().AddHours(1), KetThucDuKien = Clock.GetUtcNow().AddHours(1).AddMinutes(50) };
            Store.Them(Task); Store.Them(Session);
            var coordinator = new HocTapDieuPhoi(Store);
            var calendar = new ThoiKhoaBieuService(Store, coordinator, Clock);
            Service = new(Store, coordinator, new QuyTacDatPhien(Store, calendar), Clock);
        }
        public Task<HocTapResponse<PhienTuHocResponse>> Change(string action) =>
            Service.DoiTrangThaiAsync(1, 1, Store.User.PhienBanLapLich, action, default);
    }

    // Bộ giả lập chỉ hỗ trợ truy vấn và phiên bản; khóa/hoàn tác PostgreSQL được kiểm tra trong IntegrationChecks.
    private sealed class MemoryStore : IHocTapStore
    {
        private readonly List<object> entities = [];
        public NguoiDung User { get; } = new() { Ma = 1, PhienBanLapLich = 5 };
        public IQueryable<T> TruyVan<T>() where T : class => entities.OfType<T>().AsQueryable();
        public IQueryable<NhiemVu> NhiemVuTrongThungRac() => TruyVan<NhiemVu>().Where(t => t.NgayXoa.HasValue);
        public Task<List<T>> DanhSachAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.ToList());
        public Task<T?> DauTienAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.FirstOrDefault());
        public Task<int> DemAsync<T>(IQueryable<T> query, CancellationToken ct) => Task.FromResult(query.Count());
        public void Them<T>(T entity) where T : class => entities.Add(entity);
        public void Xoa<T>(T entity) where T : class => entities.Remove(entity);
        public Task LuuAsync(CancellationToken ct) => Task.CompletedTask;
        public Task<List<NhiemVu>> CayAsync(int chu, int ma, CancellationToken ct) => throw new NotSupportedException();
        public Task KhoaQuyTacUuTienAsync(CancellationToken ct) => Task.CompletedTask;
        public Task<IHocTapTransaction> BatDauAsync(bool ghi, CancellationToken ct) => Task.FromResult<IHocTapTransaction>(new MemoryTransaction());
        public Task<NguoiDung> NguoiDungAsync(int chu, bool khoa, CancellationToken ct) => Task.FromResult(User);
    }
    private sealed class MemoryTransaction : IHocTapTransaction
    {
        public Task CommitAsync(CancellationToken ct) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
