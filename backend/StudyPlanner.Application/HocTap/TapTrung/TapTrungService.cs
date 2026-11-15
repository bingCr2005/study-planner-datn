using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

public sealed class TapTrungService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    public async Task<HocTapResponse<TapTrungState>> DocAsync(int chu, int trang, CancellationToken ct)
    {
        if (trang is < 1 or > 50000) throw Loi("Trang lịch sử không hợp lệ.");
        // Chốt đồng hồ đếm ngược hết giờ dưới cùng khóa tài khoản với các thao tác đồng hồ khác.
        // Lần đọc kế tiếp chốt đúng mốc hết giờ, kể cả khi trình duyệt đã đóng trước đó.
        await using var tx = await store.BatDauAsync(true, ct);
        var user = await store.NguoiDungAsync(chu, true, ct);
        var now = clock.GetUtcNow();
        var current = await HienTaiAsync(chu, ct);
        if (current is not null && QuyTacTapTrung.HetGio(current) is DateTimeOffset end && now >= end)
        {
            await DongAsync(current, end, "Completed", ct);
            user.PhienBanLapLich++;
            await store.LuuAsync(ct);
            current = null;
        }
        var zone = QuyTacThoiGian.MuiGio(user.MuiGio);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        var from = QuyTacThoiGian.Utc(today, TimeOnly.MinValue, zone);
        var to = QuyTacThoiGian.Utc(today.AddDays(1), TimeOnly.MinValue, zone);
        var history = store.TruyVan<PhienTapTrung>().Where(p => p.MaNguoiDung == chu && p.TrangThai == "Completed");
        var total = await store.DemAsync(history, ct);
        var rows = await store.DanhSachAsync(history.OrderByDescending(p => p.BatDau).ThenByDescending(p => p.Ma)
            .Skip((trang - 1) * 20).Take(20), ct);
        // Thống kê hôm nay tính theo ngày hoàn tất phiên trong múi giờ của tài khoản.
        var summary = await store.DauTienAsync(history.GroupBy(p => 1).Select(g => new TongQuanTapTrung(
            g.Sum(p => p.KetThuc >= from && p.KetThuc < to && p.CheDo == "Pomodoro" ? (int)(p.SoMiliGiayDaHoc / (p.MucTieuGiay * 1000L)) : 0),
            g.Sum(p => p.KetThuc >= from && p.KetThuc < to ? p.SoMiliGiayDaHoc : 0),
            g.Sum(p => p.CheDo == "Pomodoro" ? (int)(p.SoMiliGiayDaHoc / (p.MucTieuGiay * 1000L)) : 0),
            g.Sum(p => p.SoMiliGiayDaHoc))), ct) ?? new(0, 0, 0, 0);
        var legacyId = current is null ? await store.DauTienAsync(store.TruyVan<PhienTuHoc>()
            .Where(p => p.MaNguoiDung == chu && p.TrangThai == TrangThaiPhienTuHoc.InProgress).Select(p => (int?)p.Ma), ct) : null;
        await tx.CommitAsync(ct);
        return new(user.PhienBanLapLich, new(current is null ? null : TapTrungResponse.Tu(current), rows.Select(TapTrungResponse.Tu).ToArray(),
            summary, trang, total, now, user.MuiGio, legacyId));
    }

    public async Task<HocTapResponse<TapTrungResponse>> BatDauAsync(int chu, long revision, BatDauTapTrungRequest request, CancellationToken ct)
    {
        QuyTacTapTrung.KiemTraCheDo(request.CheDo, request.MucTieuGiay);
        await using var session = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var now = clock.GetUtcNow();
        var current = await HienTaiAsync(chu, ct);
        if (current is not null)
        {
            if (current.MaPhienTuHoc != request.MaPhienTuHoc || (!request.MaPhienTuHoc.HasValue && current.MaNhiemVu != request.MaNhiemVu))
                throw XungDot("Đang có phiên tập trung khác. Hãy tiếp tục hoặc kết thúc phiên đó trước.");
            await session.CommitAsync(false, ct);
            return new(session.NguoiDung.PhienBanLapLich, TapTrungResponse.Tu(current));
        }
        PhienTuHoc? planned = null;
        if (request.MaPhienTuHoc is int sessionId)
        {
            planned = await store.DauTienAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu && p.Ma == sessionId), ct) ?? throw KhongTimThay();
            if (planned.TrangThai is not (TrangThaiPhienTuHoc.Planned or TrangThaiPhienTuHoc.InProgress) ||
                (planned.TrangThai == TrangThaiPhienTuHoc.Planned && now >= planned.KetThucDuKien))
                throw XungDot("Phiên tự học đã kết thúc, bị hủy hoặc đã hết giờ bắt đầu.");
            if (request.MaNhiemVu.HasValue && request.MaNhiemVu != planned.MaNhiemVu) throw Loi("Nhiệm vụ không khớp phiên tự học.");
        }
        if (await store.DemAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu &&
            p.TrangThai == TrangThaiPhienTuHoc.InProgress && (!request.MaPhienTuHoc.HasValue || p.Ma != request.MaPhienTuHoc)), ct) > 0)
            throw XungDot("Có phiên tự học đang chạy. Hãy mở phiên đó để tiếp tục.");
        var task = await NhiemVuAsync(chu, planned?.MaNhiemVu ?? request.MaNhiemVu, false, ct);
        var row = new PhienTapTrung { MaNguoiDung = chu, MaNhiemVu = task?.Ma, MaPhienTuHoc = planned?.Ma,
            TieuDe = task?.TieuDe ?? "Tập trung tự do", CheDo = request.CheDo, MucTieuGiay = request.MucTieuGiay,
            BatDau = now, BatDauDoan = now };
        var open = await store.DauTienAsync(store.TruyVan<KhoangThoiGianHoc>().Where(d => d.MaNguoiDung == chu && d.KetThucThucTe == null), ct);
        if (planned?.TrangThai == TrangThaiPhienTuHoc.InProgress)
        {
            if (open is not null && open.MaPhienTuHoc != planned.Ma) throw XungDot("Có đồng hồ khác chưa kết thúc.");
            // Tiếp nhận phiên đã bắt đầu qua API /study-sessions cũ và giữ nguyên thời gian đã học.
            var segments = await store.DanhSachAsync(store.TruyVan<KhoangThoiGianHoc>().Where(d => d.MaNguoiDung == chu && d.MaPhienTuHoc == planned.Ma), ct);
            row.CheDo = "Stopwatch";
            row.BatDau = segments.Count > 0 ? segments.Min(d => d.BatDauThucTe) : now;
            row.BatDauDoan = open?.BatDauThucTe ?? now;
            row.SoMiliGiayDaHoc = segments.Where(d => d.KetThucThucTe.HasValue).Sum(d => (d.KetThucThucTe!.Value - d.BatDauThucTe).Ticks / TimeSpan.TicksPerMillisecond);
            foreach (var segment in segments) segment.PhienTapTrung = row;
            if (open is null) MoKhoangHoc(row, now);
        }
        else
        {
            if (open is not null) throw XungDot("Có đồng hồ khác chưa kết thúc.");
            MoKhoangHoc(row, now);
            if (planned is not null) { planned.TrangThai = TrangThaiPhienTuHoc.InProgress; planned.NgayCapNhat = now; }
        }
        store.Them(row);
        await session.CommitAsync(true, ct);
        return new(session.NguoiDung.PhienBanLapLich, TapTrungResponse.Tu(row));
    }

    public async Task<HocTapResponse<TapTrungResponse>> ThaoTacAsync(int chu, int ma, long revision, string action, ThaoTacTapTrungRequest request, CancellationToken ct)
    {
        if (request.GhiChu?.Length > 2000) throw Loi("Ghi chú tối đa 2.000 ký tự.");
        if (action is not ("pause" or "resume" or "finish" or "discard" or "note")) throw Loi("Thao tác đồng hồ không hợp lệ.");
        await using var session = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var row = await store.DauTienAsync(store.TruyVan<PhienTapTrung>().Where(p => p.Ma == ma && p.MaNguoiDung == chu), ct) ?? throw KhongTimThay();
        var now = clock.GetUtcNow();
        var note = string.IsNullOrWhiteSpace(request.GhiChu) ? null : request.GhiChu.Trim();
        var changed = row.GhiChu != note;
        row.GhiChu = note;
        if (row.TrangThai == "Discarded" && action != "discard") throw XungDot("Phiên này đã được bỏ.");
        if (!QuyTacTapTrung.DangMo(row) && action != "note" &&
            !(action == "finish" && row.TrangThai == "Completed") && !(action == "discard" && row.TrangThai == "Discarded"))
            throw XungDot("Phiên này đã kết thúc. Hãy tải lại đồng hồ.");
        if (action != "discard" && QuyTacTapTrung.HetGio(row) is DateTimeOffset end && now >= end)
        {
            await DongAsync(row, end, "Completed", ct); changed = true;
        }
        if (action == "pause" && row.TrangThai == "Running")
        {
            await DongKhoangHocAsync(row, now, ct); row.TrangThai = "Paused"; changed = true;
        }
        else if (action == "resume" && row.TrangThai == "Paused")
        {
            await NhiemVuAsync(chu, row.MaNhiemVu, false, ct);
            if (await store.DemAsync(store.TruyVan<KhoangThoiGianHoc>().Where(d => d.MaNguoiDung == chu && d.KetThucThucTe == null), ct) > 0)
                throw XungDot("Có đồng hồ khác chưa kết thúc.");
            row.BatDauDoan = now; row.TrangThai = "Running"; MoKhoangHoc(row, now); changed = true;
        }
        else if (action == "finish" && QuyTacTapTrung.DangMo(row))
        {
            await DongAsync(row, now, "Completed", ct); changed = true;
        }
        else if (action == "discard" && QuyTacTapTrung.DangMo(row))
        {
            await DongKhoangHocAsync(row, now, ct);
            var segments = await store.DanhSachAsync(store.TruyVan<KhoangThoiGianHoc>().Where(d => d.MaNguoiDung == chu && d.MaPhienTapTrung == ma), ct);
            foreach (var segment in segments) store.Xoa(segment);
            row.TrangThai = "Discarded"; row.KetThuc = now; row.SoMiliGiayDaHoc = 0;
            if (row.MaPhienTuHoc.HasValue)
            {
                var planned = await store.DauTienAsync(store.TruyVan<PhienTuHoc>().Where(p => p.Ma == row.MaPhienTuHoc && p.MaNguoiDung == chu), ct);
                if (planned is not null) { planned.TrangThai = now < planned.KetThucDuKien ? TrangThaiPhienTuHoc.Planned : TrangThaiPhienTuHoc.Missed; planned.NgayCapNhat = now; }
            }
            changed = true;
        }
        else if ((action is "pause" or "resume") && !QuyTacTapTrung.DangMo(row) && row.TrangThai != "Completed")
            throw XungDot("Phiên đã kết thúc.");
        await session.CommitAsync(changed, ct);
        return new(session.NguoiDung.PhienBanLapLich, TapTrungResponse.Tu(row));
    }

    public async Task<HocTapResponse<TapTrungResponse>> NhapAsync(int chu, long revision, NhapTapTrungRequest r, CancellationToken ct)
    {
        QuyTacTapTrung.KiemTraCheDo(r.CheDo, r.MucTieuGiay);
        var now = clock.GetUtcNow();
        if (r.MaPhienTuHoc.HasValue || r.KetThuc <= r.BatDau || r.KetThuc > now || r.KetThuc - r.BatDau > TimeSpan.FromDays(1) || r.GhiChu?.Length > 2000)
            throw Loi("Nhập khoảng đã học trong quá khứ, dài tối đa 24 giờ và ghi chú tối đa 2.000 ký tự.");
        await using var session = await dieuPhoi.MoAsync(chu, revision, true, ct);
        if (await store.DemAsync(store.TruyVan<PhienTapTrung>().Where(p => p.MaNguoiDung == chu && p.TrangThai != "Discarded" &&
            p.BatDau < r.KetThuc && (!p.KetThuc.HasValue || p.KetThuc > r.BatDau)), ct) > 0 ||
            await store.DemAsync(store.TruyVan<KhoangThoiGianHoc>().Where(d => d.MaNguoiDung == chu && d.BatDauThucTe < r.KetThuc &&
                (!d.KetThucThucTe.HasValue || d.KetThucThucTe > r.BatDau)), ct) > 0)
            throw XungDot("Khoảng nhập trùng với thời gian đã ghi hoặc phiên đang mở.");
        var task = await NhiemVuAsync(chu, r.MaNhiemVu, true, ct);
        var row = new PhienTapTrung { MaNguoiDung = chu, MaNhiemVu = task?.Ma, TieuDe = task?.TieuDe ?? "Tập trung tự do",
            CheDo = r.CheDo, MucTieuGiay = r.MucTieuGiay, TrangThai = "Completed", BatDau = r.BatDau.ToUniversalTime(),
            KetThuc = r.KetThuc.ToUniversalTime(), SoMiliGiayDaHoc = (r.KetThuc - r.BatDau).Ticks / TimeSpan.TicksPerMillisecond,
            GhiChu = string.IsNullOrWhiteSpace(r.GhiChu) ? null : r.GhiChu.Trim(), NhapThuCong = true };
        if (task is not null) store.Them(new KhoangThoiGianHoc { MaNguoiDung = chu, MaNhiemVu = task.Ma, BatDauThucTe = row.BatDau,
            KetThucThucTe = row.KetThuc, PhienTapTrung = row });
        store.Them(row);
        await session.CommitAsync(true, ct);
        return new(session.NguoiDung.PhienBanLapLich, TapTrungResponse.Tu(row));
    }

    private Task<PhienTapTrung?> HienTaiAsync(int chu, CancellationToken ct) => store.DauTienAsync(store.TruyVan<PhienTapTrung>()
        .Where(p => p.MaNguoiDung == chu && (p.TrangThai == "Running" || p.TrangThai == "Paused")), ct);
    private async Task<NhiemVu?> NhiemVuAsync(int chu, int? id, bool allowCompleted, CancellationToken ct)
    {
        if (!id.HasValue) return null;
        var task = await store.DauTienAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && t.Ma == id && t.NgayXoa == null), ct) ?? throw KhongTimThay();
        if (!allowCompleted && (task.TrangThai == TrangThaiNhiemVu.Completed ||
            await store.DemAsync(store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && t.MaNhiemVuCha == id), ct) > 0))
            throw XungDot("Chọn nhiệm vụ chưa hoàn thành và chưa được chia nhỏ. Với nhiệm vụ lớn, chọn một bước cụ thể.");
        return task;
    }
    private void MoKhoangHoc(PhienTapTrung row, DateTimeOffset now)
    {
        if (row.MaNhiemVu is int task) store.Them(new KhoangThoiGianHoc { MaNguoiDung = row.MaNguoiDung, MaNhiemVu = task,
            MaPhienTuHoc = row.MaPhienTuHoc, PhienTapTrung = row, BatDauThucTe = now });
    }
    private async Task DongKhoangHocAsync(PhienTapTrung row, DateTimeOffset now, CancellationToken ct)
    {
        row.SoMiliGiayDaHoc = QuyTacTapTrung.ThoiLuong(row, now); row.BatDauDoan = null;
        var open = await store.DanhSachAsync(store.TruyVan<KhoangThoiGianHoc>().Where(d => d.MaNguoiDung == row.MaNguoiDung &&
            d.MaPhienTapTrung == row.Ma && d.KetThucThucTe == null), ct);
        foreach (var segment in open) segment.KetThucThucTe = now > segment.BatDauThucTe ? now : segment.BatDauThucTe.AddTicks(10);
    }
    private async Task DongAsync(PhienTapTrung row, DateTimeOffset now, string state, CancellationToken ct)
    {
        await DongKhoangHocAsync(row, now, ct); row.TrangThai = state; row.KetThuc = now;
        if (row.MaPhienTuHoc.HasValue)
        {
            var planned = await store.DauTienAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == row.MaNguoiDung && p.Ma == row.MaPhienTuHoc), ct);
            if (planned is not null) { planned.TrangThai = TrangThaiPhienTuHoc.Completed; planned.NgayCapNhat = now; }
        }
    }
    private static HocTapException Loi(string text) => new(400, "VALIDATION_ERROR", text);
    private static HocTapException XungDot(string text) => new(409, "FOCUS_STATE_CONFLICT", text);
    private static HocTapException KhongTimThay() => new(404, "RESOURCE_NOT_FOUND", "Không tìm thấy phiên hoặc nhiệm vụ của bạn.");
}
