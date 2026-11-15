using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.CanhBao;

public sealed class CauHinhThongBaoRequest
{
    [JsonRequired, Range(0, 8760)] public int NhacTruocGio { get; init; }
    [JsonRequired] public bool NhacKeHoach { get; init; }
}
public sealed record CauHinhThongBaoResponse(int NhacTruocGio, bool NhacKeHoach);

/// <summary>Một nơi cấu hình nhắc hạn và nhắc kế hoạch; dùng lại dữ liệu đã có, không tạo bản sao cấu hình.</summary>
public sealed class CauHinhThongBaoService(IHocTapStore store, HocTapDieuPhoi dieuPhoi, TimeProvider clock)
{
    public async Task<HocTapResponse<CauHinhThongBaoResponse>> DocAsync(int owner, CancellationToken ct)
    {
        await using var tx = await dieuPhoi.MoAsync(owner, null, false, ct);
        var settings = await store.DauTienAsync(store.TruyVan<CauHinhNhacKeHoach>().Where(x => x.MaNguoiDung == owner), ct);
        await tx.CommitAsync(false, ct);
        return new(tx.NguoiDung.PhienBanLapLich, new(tx.NguoiDung.NhacTruocGio, settings?.DangBat ?? false));
    }

    public async Task<HocTapResponse<CauHinhThongBaoResponse>> LuuAsync(int owner, long version, CauHinhThongBaoRequest request, CancellationToken ct)
    {
        if (request.NhacTruocGio is < 0 or > 8760)
            throw new HocTapException(400, "VALIDATION_ERROR", "Thời gian nhắc trước hạn từ 0 đến 8.760 giờ.");
        await using var tx = await dieuPhoi.MoAsync(owner, version, true, ct);
        var settings = await store.DauTienAsync(store.TruyVan<CauHinhNhacKeHoach>().Where(x => x.MaNguoiDung == owner), ct);
        var changed = tx.NguoiDung.NhacTruocGio != request.NhacTruocGio || (settings?.DangBat ?? false) != request.NhacKeHoach;
        if (changed)
        {
            var now = clock.GetUtcNow();
            tx.NguoiDung.NhacTruocGio = request.NhacTruocGio;
            tx.NguoiDung.NgayCapNhat = now;
            if (settings is not null) { settings.DangBat = request.NhacKeHoach; settings.NgayCapNhat = now; }
            else if (request.NhacKeHoach) store.Them(new CauHinhNhacKeHoach { MaNguoiDung = owner, DangBat = true, NgayTao = now, NgayCapNhat = now });
        }
        // Lưu chung hai lựa chọn dưới khóa tài khoản; phiên bản cũ không được ghi đè thay đổi của tab khác.
        await tx.CommitAsync(changed, ct);
        return new(tx.NguoiDung.PhienBanLapLich, new(request.NhacTruocGio, request.NhacKeHoach));
    }
}
