using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
namespace StudyPlanner.Api.Controllers;

[Route("api/v1/focus")]
public sealed class TapTrungController(TapTrungService service, ThongKeTapTrungService statistics) : HocTapControllerBase
{
    [HttpGet("statistics")]
    public async Task<HocTapResponse<ThongKeTapTrungResponse>> ThongKe([FromQuery] DateOnly? ngay = null, [FromQuery] int soNgay = 7, CancellationToken ct = default)
    { var r = await statistics.DocAsync(Chu, ngay, soNgay, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("tasks/{id:int}/summary")]
    public async Task<HocTapResponse<TongHopTapTrung>> NhiemVu(int id, CancellationToken ct)
    { var r = await statistics.NhiemVuAsync(Chu, id, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("state")]
    public async Task<HocTapResponse<TapTrungState>> Doc([FromQuery] int trang = 1, CancellationToken ct = default)
    { var r = await service.DocAsync(Chu, trang, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost("start")]
    public async Task<HocTapResponse<TapTrungResponse>> BatDau(BatDauTapTrungRequest request, CancellationToken ct)
    { var r = await service.BatDauAsync(Chu, PhienBanYeuCau(), request, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost("records")]
    public async Task<HocTapResponse<TapTrungResponse>> Nhap(NhapTapTrungRequest request, CancellationToken ct)
    { var r = await service.NhapAsync(Chu, PhienBanYeuCau(), request, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost("{id:int}/{operation}")]
    public async Task<HocTapResponse<TapTrungResponse>> ThaoTac(int id, string operation, ThaoTacTapTrungRequest request, CancellationToken ct)
    { var r = await service.ThaoTacAsync(Chu, id, PhienBanYeuCau(), operation, request, ct); GanPhienBan(r.PhienBan); return r; }
}
