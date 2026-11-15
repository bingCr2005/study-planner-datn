using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.HocTap;

public sealed record CauHinhHocResponse(int PhienHocPhut, int NghiGiuaPhienPhut, int GioiHanHocMoiNgayPhut)
{
    public static CauHinhHocResponse Tu(NguoiDung user) => new(user.PhienHocPhut, user.NghiGiuaPhienPhut, user.GioiHanHocMoiNgayPhut);
    public static CauHinhHocResponse MacDinh { get; } = new(25, 5, 180);
    public void KiemTra()
    {
        if (PhienHocPhut is < 15 or > 50 || NghiGiuaPhienPhut is < 1 or > 30 ||
            GioiHanHocMoiNgayPhut is < 15 or > 720 || GioiHanHocMoiNgayPhut < PhienHocPhut)
            throw new HocTapException(400, "VALIDATION_ERROR", "Phiên học 15–50 phút, nghỉ 1–30 phút; giới hạn ngày 15–720 phút và không ngắn hơn một phiên.");
    }
}

public sealed class CauHinhHocRequest
{
    [JsonRequired, Range(15, 50)] public int PhienHocPhut { get; init; } = 25;
    [JsonRequired, Range(1, 30)] public int NghiGiuaPhienPhut { get; init; } = 5;
    [JsonRequired, Range(15, 720)] public int GioiHanHocMoiNgayPhut { get; init; } = 180;
}

public sealed class CauHinhHocService(HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    public async Task<HocTapResponse<CauHinhHocResponse>> DocAsync(int chu, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(chu, null, false, ct);
        var result = CauHinhHocResponse.Tu(tx.NguoiDung);
        await tx.CommitAsync(false, ct);
        return new(tx.NguoiDung.PhienBanLapLich, result);
    }
    public async Task<HocTapResponse<CauHinhHocResponse>> LuuAsync(int chu, long revision, CauHinhHocRequest request, CancellationToken ct)
    {
        var next = new CauHinhHocResponse(request.PhienHocPhut, request.NghiGiuaPhienPhut, request.GioiHanHocMoiNgayPhut); next.KiemTra();
        await using var tx = await dieuPhoi.MoAsync(chu, revision, true, ct);
        var user = tx.NguoiDung; var changed = CauHinhHocResponse.Tu(user) != next;
        user.PhienHocPhut = next.PhienHocPhut; user.NghiGiuaPhienPhut = next.NghiGiuaPhienPhut; user.GioiHanHocMoiNgayPhut = next.GioiHanHocMoiNgayPhut;
        if (changed) user.NgayCapNhat = clock.GetUtcNow();
        // Đổi cấu hình làm phiếu gợi ý cũ hết hiệu lực qua phiên bản nguồn; không sửa phiên đã đặt hay đồng hồ đang chạy.
        await tx.CommitAsync(changed, ct);
        return new(user.PhienBanLapLich, next);
    }
}

public static class QuyTacGioiHanNgay
{
    public static IEnumerable<(DateOnly Ngay, double Phut)> Tach(KhoangThoiGian range, TimeZoneInfo zone)
    {
        // Chia theo ngày địa phương nhưng đo số phút thực giữa các mốc UTC để xử lý đúng ngày đổi giờ mùa hè.
        var cursor = range.BatDau;
        while (cursor < range.KetThuc)
        {
            var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(cursor, zone).DateTime);
            var end = QuyTacThoiGian.Utc(day.AddDays(1), TimeOnly.MinValue, zone);
            if (end > range.KetThuc) end = range.KetThuc;
            yield return (day, (end - cursor).TotalMinutes);
            cursor = end;
        }
    }
    public static Dictionary<DateOnly, double> Tong(IEnumerable<KhoangThoiGian> ranges, TimeZoneInfo zone) =>
        ranges.SelectMany(range => Tach(range, zone)).GroupBy(row => row.Ngay).ToDictionary(g => g.Key, g => g.Sum(row => row.Phut));
}
