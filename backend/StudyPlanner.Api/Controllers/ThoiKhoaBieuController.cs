using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
namespace StudyPlanner.Api.Controllers;

[Route("api/v1/calendar")]
public sealed class ThoiKhoaBieuController(ThoiKhoaBieuService service, LichHocService lich, KhungGioRanhService ranh) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<ThoiKhoaBieuResponse>> DanhSach([FromQuery] DateTimeOffset tu, [FromQuery] DateTimeOffset den,
        [FromQuery] int? maHocKy, [FromQuery] int? maMonHoc, [FromQuery] bool hienHanCha, CancellationToken ct)
    { var r = await service.LayAsync(Chu, tu, den, maHocKy, maMonHoc, hienHanCha, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost("change-preview")]
    public async Task<HocTapResponse<AnhHuongLichResponse>> XemAnhHuong(XemAnhHuongLichRequest request, CancellationToken ct)
    {
        var r = request.Loai switch
        {
            "LichHoc" => await lich.XemAnhHuongAsync(Chu, request.Ma, request.MaMonHoc, request.LichHoc, request.Xoa, ct),
            "GioRanh" => await ranh.XemAnhHuongAsync(Chu, request.Ma, request.KhungGioRanh, request.Xoa, ct),
            _ => throw new HocTapException(400, "VALIDATION_ERROR", "Loại xem trước phải là LichHoc hoặc GioRanh; sự kiện nhiệm vụ dùng tasks/{id}/change-preview.")
        };
        GanPhienBan(r.PhienBan); return r;
    }
}
