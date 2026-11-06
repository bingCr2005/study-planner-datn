using System.Security.Cryptography;
using System.Text.Json;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.HocTap;

public sealed class LapLichService(IHocTapStore store, HocTapDieuPhoi dieuPhoi,
    ThoiKhoaBieuService lich, QuyTacDatPhien datPhien, IPreviewLapLichToken tokens, TimeProvider clock)
{
    public async Task<HocTapResponse<XemTruocLichResponse>> XemTruocAsync(int chu, XemTruocLichRequest request, CancellationToken ct)
    {
        if (request.SoNgay is < 1 or > 31 || request.DanhSachMaNhiemVu is null ||
            request.DanhSachMaNhiemVu.Count > 1000 || request.DanhSachMaNhiemVu.Any(x => x <= 0))
            throw new HocTapException(400, "VALIDATION_ERROR", "Khoảng lập lịch 1–31 ngày, tối đa 1.000 nhiệm vụ hợp lệ.");
        await using var session = await dieuPhoi.MoAsync(chu, null, false, ct);
        var now = clock.GetUtcNow();
        var zone = QuyTacThoiGian.MuiGio(session.NguoiDung.MuiGio);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        var startDay = request.NgayBatDau ?? today;
        if (startDay < today || startDay.DayNumber > DateOnly.MaxValue.DayNumber - request.SoNgay)
            throw new HocTapException(400, "INVALID_DATE_RANGE", "Ngày bắt đầu không được trong quá khứ hoặc vượt giới hạn lịch.");
        var from = QuyTacThoiGian.Utc(startDay, TimeOnly.MinValue, zone);
        var to = QuyTacThoiGian.Utc(startDay.AddDays(request.SoNgay), TimeOnly.MinValue, zone);
        if (request.MaHocKy is int semester && await store.DemAsync(store.TruyVan<HocKy>().Where(s => s.Ma == semester && s.MaNguoiDung == chu), ct) == 0)
            throw KhongTimThay();
        if (request.MaMonHoc is int course && await store.DemAsync(store.TruyVan<MonHoc>().Where(m => m.Ma == course && m.HocKy.MaNguoiDung == chu &&
            (!request.MaHocKy.HasValue || m.MaHocKy == request.MaHocKy)), ct) == 0) throw KhongTimThay();

        var selected = new HashSet<int>();
        foreach (var id in request.DanhSachMaNhiemVu.Distinct())
        {
            var tree = await store.CayAsync(chu, id, ct);
            if (tree.All(t => t.Ma != id)) throw KhongTimThay();
            var children = tree.ToLookup(t => t.MaNhiemVuCha);
            void Visit(int current)
            {
                if (!selected.Add(current)) return;
                foreach (var child in children[current]) Visit(child.Ma);
            }
            Visit(id);
            if (selected.Count > 1000) throw new HocTapException(400, "PLANNING_LIMIT", "Phạm vi vượt 1.000 nhiệm vụ; hãy thu hẹp.");
        }
        var chosenIds = selected.ToArray();
        var query = store.TruyVan<NhiemVu>().Where(t => t.MaNguoiDung == chu && t.TrangThai != TrangThaiNhiemVu.Completed &&
            !t.DanhSachNhiemVuCon.Any() && (!request.MaHocKy.HasValue || t.MaHocKy == request.MaHocKy) &&
            (!request.MaMonHoc.HasValue || t.MaMonHoc == request.MaMonHoc) && (chosenIds.Length == 0 || chosenIds.Contains(t.Ma)));
        var tasks = await store.DanhSachAsync(query.OrderBy(t => t.Ma).Take(1001), ct);
        if (tasks.Count > 1000) throw new HocTapException(400, "PLANNING_LIMIT", "Phạm vi vượt 1.000 lá; hãy chọn học kỳ/môn hoặc nhóm việc nhỏ hơn.");
        var map = tasks.ToDictionary(t => t.Ma);
        await QuyTacDatPhien.NapToTienAsync(store, chu, map, ct);
        var taskIds = tasks.Select(t => t.Ma).ToArray();
        var semesters = tasks.Where(t => t.MaHocKy.HasValue).Select(t => t.MaHocKy!.Value).Distinct().ToArray();
        var archived = (await store.DanhSachAsync(store.TruyVan<HocKy>().Where(s => s.MaNguoiDung == chu && semesters.Contains(s.Ma) && s.DaLuuTru)
            .Select(s => s.Ma), ct)).ToHashSet();
        var reserved = await store.DanhSachAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu && taskIds.Contains(p.MaNhiemVu) &&
            p.KetThucDuKien > now && (p.TrangThai == TrangThaiPhienTuHoc.Planned || p.TrangThai == TrangThaiPhienTuHoc.InProgress)), ct);
        var warnings = new List<CanhBaoLapLich>();
        var unreviewed = await store.DanhSachAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu &&
            taskIds.Contains(p.MaNhiemVu) && p.TrangThai == TrangThaiPhienTuHoc.Planned && p.KetThucDuKien <= now)
            .Select(p => p.MaNhiemVu).Distinct().Take(1001), ct);
        foreach (var task in tasks.Where(t => unreviewed.Contains(t.Ma)))
            warnings.Add(new(task.Ma, task.TieuDe, "PAST_SESSION_NEEDS_REVIEW",
                "Có phiên Planned đã qua giờ. Hãy xác nhận đã kết thúc hoặc bỏ lỡ; không tính phiên đó là thời lượng đã học.", 0));
        var jobs = new List<CongViecLapLich>();
        var sumRequired = 0; var sumReserved = 0;
        foreach (var task in tasks)
        {
            var due = QuyTacCay.HanHieuLuc(task, map).Han;
            string? reason = null; string? message = null;
            if (task.MaHocKy is int sid && archived.Contains(sid)) { reason = "SEMESTER_ARCHIVED"; message = "Học kỳ đã lưu trữ."; }
            else if (!task.ThoiLuongDuKienPhut.HasValue) { reason = "MISSING_ESTIMATE"; message = "Chưa nhập thời lượng dự kiến."; }
            else if (due.HasValue && due <= now) { reason = "TASK_OVERDUE"; message = "Hạn đã qua; hãy điều chỉnh hạn trước khi xếp lịch."; }
            var remaining = task.ThoiLuongConLaiPhut ?? 0;
            if (reason is not null) { warnings.Add(new(task.Ma, task.TieuDe, reason, message!, remaining)); continue; }
            var booked = reserved.Where(p => p.MaNhiemVu == task.Ma)
                .Sum(p => Math.Max(0, (p.KetThucDuKien - (p.BatDauDuKien < now ? now : p.BatDauDuKien)).TotalMinutes));
            // Chỉ xếp phút nguyên; không làm tròn riêng từng phiên khiến phần đã dành chỗ bị trừ hai lần.
            var need = Math.Max(0, (int)Math.Floor(remaining - booked));
            sumRequired = checked(sumRequired + need);
            sumReserved = checked(sumReserved + (int)Math.Ceiling(booked));
            if (need == 0) warnings.Add(new(task.Ma, task.TieuDe, "ALREADY_RESERVED", "Đã đủ thời lượng trong các phiên còn hiệu lực.", 0));
            else jobs.Add(new(task.Ma, task.TieuDe, due, task.MucDoUuTien, need));
        }
        var source = await lich.LayNguonAsync(chu, from.AddMinutes(-10), to.AddMinutes(10), ct);
        var busy = source.LichHoc.Concat(source.SuKien).Concat(source.PhienTuHoc
            .Where(p => p.TrangThai != TrangThaiPhienTuHoc.Cancelled)
            .Select(p => new KhoangThoiGian(p.BatDauDuKien.AddMinutes(-10), p.KetThucDuKien.AddMinutes(10))));
        var free = QuyTacThoiGian.Tru(source.GioRanh, busy)
            .Select(g => new KhoangThoiGian(g.BatDau < from ? from : g.BatDau, g.KetThuc > to ? to : g.KetThuc))
            .Where(g => g.KetThuc > g.BatDau).ToList();
        var result = QuyTacLapLich.Xep(jobs, free, now);
        foreach (var job in jobs.Where(j => result.ChuaXep[j.MaNhiemVu] > 0))
            warnings.Add(new(job.MaNhiemVu, job.TieuDe, free.Count == 0 ? "NO_AVAILABILITY" : "INSUFFICIENT_TIME",
                free.Count == 0 ? "Không có giờ rảnh khả dụng trong khoảng chọn." : "Chưa đủ chỗ trước hạn hoặc không đủ độ dài phiên/nghỉ.", result.ChuaXep[job.MaNhiemVu]));
        if (await store.DemAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu && p.TrangThai == TrangThaiPhienTuHoc.InProgress && p.KetThucDuKien <= now), ct) > 0)
            warnings.Add(new(0, "Phiên đang học", "SESSION_NEEDS_REVIEW", "Có phiên đang học đã quá giờ; cần kết thúc trước khi áp dụng lịch mới.", 0));
        var expires = now.AddMinutes(10);
        var payload = new NoiDungPreviewLapLich(chu, session.NguoiDung.PhienBanLapLich, QuyTacLapLich.PhienBan, now, expires,
            result.Phien.Select(p => new ChonPhienTuHocRequest(p.MaNhiemVu, p.BatDauDuKien, p.KetThucDuKien)).ToList());
        var response = new XemTruocLichResponse(result.Phien, warnings, sumRequired, sumReserved,
            result.Phien.Sum(p => (int)(p.KetThucDuKien - p.BatDauDuKien).TotalMinutes), result.ChuaXep.Values.Sum(),
            payload.PhienBanNguon, payload.PhienBanQuyTac, now, expires, tokens.Tao(payload), session.NguoiDung.MuiGio);
        await session.CommitAsync(false, ct);
        return new(session.NguoiDung.PhienBanLapLich, response);
    }

    public async Task<HocTapResponse<ApDungLichResponse>> ApDungAsync(int chu, string key, ApDungLichRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || request.Phien is null || request.Phien.Count is < 1 or > 2000)
            throw new HocTapException(400, "VALIDATION_ERROR", "Cần Idempotency-Key tối đa 128 ký tự và 1–2.000 phiên.");
        var payload = tokens.Doc(request.TokenPreview);
        if (payload.MaNguoiDung != chu) throw KhongTimThay();
        var selected = request.Phien.OrderBy(p => p.BatDauDuKien).ThenBy(p => p.KetThucDuKien).ThenBy(p => p.MaNhiemVu).ToArray();
        if (selected.Distinct().Count() != selected.Length || selected.Any(p => !payload.Phien.Contains(p)))
            throw new HocTapException(400, "INVALID_PREVIEW_SELECTION", "Chỉ được chọn một tập con nguyên vẹn của lịch đề xuất.");
        var hash = Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { payload.PhienBanNguon, payload.PhienBanQuyTac, phien = selected.Select(p => new { p.MaNhiemVu, batDau = p.BatDauDuKien.UtcTicks, ketThuc = p.KetThucDuKien.UtcTicks }) })));
        // Apply mở khóa chưa kiểm tra revision để tìm key thành công trước; retry sau timeout không bị coi là dữ liệu cũ.
        await using var tx = await store.BatDauAsync(true, ct);
        var user = await store.NguoiDungAsync(chu, true, ct);
        var previous = await store.DauTienAsync(store.TruyVan<LanApDungLich>().Where(b => b.MaNguoiDung == chu && b.KhoaYeuCau == key), ct);
        if (previous is not null)
        {
            if (previous.DauKiemNoiDung != hash)
                throw new HocTapException(409, "IDEMPOTENCY_KEY_REUSED", "Khóa yêu cầu đã dùng cho nội dung khác.");
            var old = await store.DanhSachAsync(store.TruyVan<PhienTuHoc>().Where(p => p.MaNguoiDung == chu && p.MaLanApDung == previous.Ma).OrderBy(p => p.BatDauDuKien), ct);
            await tx.CommitAsync(ct);
            return new(user.PhienBanLapLich, new(previous.Ma, true, old.Select(PhienTuHocResponse.Tu).ToList()));
        }
        var now = clock.GetUtcNow();
        if (payload.PhienBanNguon != user.PhienBanLapLich)
            throw new HocTapException(409, "DATA_VERSION_CONFLICT", "Dữ liệu đã đổi sau khi xem gợi ý; hãy tạo gợi ý mới.");
        if (payload.PhienBanQuyTac != QuyTacLapLich.PhienBan)
            throw new HocTapException(409, "RULE_VERSION_CONFLICT", "Quy tắc đã đổi; hãy tạo gợi ý mới.");
        if (now > payload.HetHanLuc || payload.ThoiDiemTinh > now)
            throw new HocTapException(409, "PREVIEW_EXPIRED", "Gợi ý đã hết hạn; hãy tính lại.");
        await datPhien.KiemTraAsync(chu, selected, now, null, ct);
        var batch = new LanApDungLich { MaNguoiDung = chu, KhoaYeuCau = key, DauKiemNoiDung = hash,
            PhienBanNguon = payload.PhienBanNguon, PhienBanQuyTac = payload.PhienBanQuyTac, NgayTao = now };
        store.Them(batch); await store.LuuAsync(ct);
        var added = selected.Select(p => new PhienTuHoc { MaNguoiDung = chu, MaNhiemVu = p.MaNhiemVu,
            MaLanApDung = batch.Ma, BatDauDuKien = p.BatDauDuKien.ToUniversalTime(), KetThucDuKien = p.KetThucDuKien.ToUniversalTime(),
            TrangThai = TrangThaiPhienTuHoc.Planned, NgayTao = now, NgayCapNhat = now }).ToList();
        foreach (var p in added) store.Them(p);
        user.PhienBanLapLich = checked(user.PhienBanLapLich + 1);
        await store.LuuAsync(ct); await tx.CommitAsync(ct);
        return new(user.PhienBanLapLich, new(batch.Ma, false, added.Select(PhienTuHocResponse.Tu).ToList()));
    }

    private static HocTapException KhongTimThay() => new(404, "RESOURCE_NOT_FOUND", "Không tìm thấy tài nguyên.");
}
