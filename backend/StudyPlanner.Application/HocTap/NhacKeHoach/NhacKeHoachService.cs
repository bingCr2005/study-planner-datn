using System.Text.Json.Serialization;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.HocTap;

public sealed class CauHinhNhacKeHoachRequest
{
    [JsonRequired] public bool DangBat { get; init; }
}
public sealed record CauHinhNhacKeHoachResponse(bool DangBat);
public sealed record NhacKeHoachResponse(int Ma, string Loai, string TieuDe, string NoiDung,
    string DuongDan, DateTimeOffset NgayPhat, DateTimeOffset? NgayDoc);
public sealed record HopNhacKeHoachResponse(IReadOnlyList<NhacKeHoachResponse> DanhSach, int Trang,
    int KichThuocTrang, int TongSo, int TongChuaDoc, string MuiGio, DateTimeOffset DocDenLuc);

/// <summary>Tạo nhắc khi ứng dụng tải hộp thông báo; không tự tạo giờ rảnh hoặc phiên học.</summary>
public sealed class NhacKeHoachService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    public async Task<HocTapResponse<CauHinhNhacKeHoachResponse>> CauHinhAsync(int owner, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(owner, null, false, ct);
        var row = await store.DauTienAsync(store.TruyVan<CauHinhNhacKeHoach>().Where(x => x.MaNguoiDung == owner), ct);
        await tx.CommitAsync(false, ct);
        return new(tx.NguoiDung.PhienBanLapLich, new(row?.DangBat ?? false));
    }

    public async Task<HocTapResponse<CauHinhNhacKeHoachResponse>> LuuCauHinhAsync(int owner, long version,
        CauHinhNhacKeHoachRequest request, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(owner, version, true, ct);
        var row = await store.DauTienAsync(store.TruyVan<CauHinhNhacKeHoach>().Where(x => x.MaNguoiDung == owner), ct);
        var changed = (row?.DangBat ?? false) != request.DangBat;
        if (changed)
        {
            var now = clock.GetUtcNow();
            if (row is null) { row = new() { MaNguoiDung = owner, NgayTao = now }; store.Them(row); }
            row.DangBat = request.DangBat; row.NgayCapNhat = now;
        }
        await tx.CommitAsync(changed, ct);
        return new(tx.NguoiDung.PhienBanLapLich, new(request.DangBat));
    }

    public async Task<HocTapResponse<HopNhacKeHoachResponse>> DanhSachAsync(int owner, bool unread, int page,
        int size, CancellationToken ct)
    {
        if (page < 1 || size is < 1 or > 50 || (long)(page - 1) * size > int.MaxValue)
            throw new HocTapException(400, "VALIDATION_ERROR", "Phân trang nhắc kế hoạch không hợp lệ.");
        await using var tx = await store.BatDauAsync(true, ct);
        // Tải hộp nhắc có thể tạo dữ liệu, nên dùng cùng khóa tài khoản để các yêu cầu đồng thời không sinh trùng.
        var user = await store.NguoiDungAsync(owner, true, ct);
        await DongBoAsync(user, ct);
        var query = store.TruyVan<NhacKeHoach>().Where(x => x.MaNguoiDung == owner);
        var unreadCount = await store.DemAsync(query.Where(x => x.NgayDoc == null), ct);
        if (unread) query = query.Where(x => x.NgayDoc == null);
        var count = await store.DemAsync(query, ct);
        var rows = await store.DanhSachAsync(query.OrderByDescending(x => x.NgayPhat).ThenByDescending(x => x.Ma)
            .Skip((page - 1) * size).Take(size).Select(x => new NhacKeHoachResponse(x.Ma, x.Loai, x.TieuDe,
                x.NoiDung, x.DuongDan, x.NgayPhat, x.NgayDoc)), ct);
        await tx.CommitAsync(ct);
        return new(user.PhienBanLapLich, new(rows, page, size, count, unreadCount, user.MuiGio, clock.GetUtcNow()));
    }

    public async Task DocAsync(int owner, int id, DateTimeOffset snapshot, CancellationToken ct)
    {
        await using var tx = await store.BatDauAsync(true, ct);
        await store.NguoiDungAsync(owner, true, ct);
        var row = await store.DauTienAsync(store.TruyVan<NhacKeHoach>().Where(x => x.MaNguoiDung == owner && x.Ma == id), ct)
            ?? throw new HocTapException(404, "RESOURCE_NOT_FOUND", "Không tìm thấy nhắc kế hoạch.");
        if (row.NgayPhat != snapshot) throw new HocTapException(409, "NOTIFICATION_CHANGED", "Nhắc kế hoạch đã thay đổi. Hãy tải lại.");
        row.NgayDoc ??= clock.GetUtcNow() < row.NgayPhat ? row.NgayPhat : clock.GetUtcNow();
        await store.LuuAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task DocTatCaAsync(int owner, DateTimeOffset cutoff, CancellationToken ct)
    {
        if (cutoff > clock.GetUtcNow()) throw new HocTapException(400, "VALIDATION_ERROR", "Mốc đọc nhắc kế hoạch không hợp lệ.");
        await using var tx = await store.BatDauAsync(true, ct);
        await store.NguoiDungAsync(owner, true, ct);
        var rows = await store.DanhSachAsync(store.TruyVan<NhacKeHoach>().Where(x => x.MaNguoiDung == owner && x.NgayDoc == null && x.NgayPhat <= cutoff).Take(10001), ct);
        if (rows.Count > 10000) throw new HocTapException(400, "QUERY_LIMIT", "Có quá nhiều nhắc kế hoạch chưa đọc. Hãy xử lý theo từng trang.");
        var now = clock.GetUtcNow();
        foreach (var row in rows) row.NgayDoc = now < row.NgayPhat ? row.NgayPhat : now;
        await store.LuuAsync(ct); await tx.CommitAsync(ct);
    }

    private async Task DongBoAsync(NguoiDung user, CancellationToken ct)
    {
        var enabled = await store.DauTienAsync(store.TruyVan<CauHinhNhacKeHoach>().Where(x => x.MaNguoiDung == user.Ma), ct);
        if (enabled?.DangBat != true) return;
        var now = clock.GetUtcNow();
        var zone = QuyTacThoiGian.MuiGio(user.MuiGio);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        // Khóa tuần dựa trên thứ Hai ở múi giờ tài khoản, tránh nhắc lại mỗi lần mở ứng dụng.
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var until = today.DayNumber <= DateOnly.MaxValue.DayNumber - 7 ? today.AddDays(7) : DateOnly.MaxValue;
        var windows = await store.DanhSachAsync(store.TruyVan<KhungGioRanh>().Where(x => x.MaNguoiDung == user.Ma && x.DangSuDung &&
            x.NgayBatDau <= until && x.NgayKetThuc >= today && x.NgayKetThuc <= until).OrderBy(x => x.Ma).Take(10001), ct);
        if (windows.Count > 10000) throw new HocTapException(400, "QUERY_LIMIT", "Có quá nhiều mẫu giờ rảnh để nhắc. Hãy thu hẹp dữ liệu.");
        var candidates = new List<NhacKeHoach> { new() { MaNguoiDung = user.Ma,
            KhoaChongTrung = $"week:{monday:yyyy-MM-dd}", Loai = "WeeklyReview", TieuDe = "Rà soát kế hoạch tuần này",
            NoiDung = $"Kiểm tra nhiệm vụ và giờ rảnh của tuần bắt đầu {monday:dd/MM/yyyy}, rồi cập nhật gợi ý lịch học khi cần.",
            DuongDan = "/tasks?tab=planning", NgayPhat = now } };
        // Nhiều mẫu thứ Hai–thứ Sáu cùng hết hạn chỉ tạo một nhắc để tránh làm đầy hộp thông báo.
        foreach (var group in windows.GroupBy(x => x.NgayKetThuc)) candidates.Add(new() { MaNguoiDung = user.Ma,
            KhoaChongTrung = $"availability-expiry:{group.Key:yyyy-MM-dd}", Loai = "AvailabilityExpiring",
            TieuDe = "Giờ rảnh sắp hết hiệu lực", NoiDung = $"Có {group.Count()} mẫu giờ rảnh áp dụng đến {group.Key:dd/MM/yyyy}. Kiểm tra và gia hạn nếu lịch này vẫn phù hợp.",
            DuongDan = "/tasks?tab=free", NgayPhat = now });
        var keys = candidates.Select(x => x.KhoaChongTrung).ToArray();
        var existing = (await store.DanhSachAsync(store.TruyVan<NhacKeHoach>().Where(x => x.MaNguoiDung == user.Ma && keys.Contains(x.KhoaChongTrung))
            .Select(x => x.KhoaChongTrung), ct)).ToHashSet();
        foreach (var row in candidates.Where(x => !existing.Contains(x.KhoaChongTrung))) store.Them(row);
        if (candidates.Any(x => !existing.Contains(x.KhoaChongTrung))) await store.LuuAsync(ct);
    }
}
