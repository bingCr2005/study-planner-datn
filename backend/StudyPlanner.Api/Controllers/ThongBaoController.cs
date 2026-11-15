using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Infrastructure.CanhBao;
using StudyPlanner.Application.CanhBao;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Api.Controllers;

[Route("api/v1/notifications")]
public sealed class ThongBaoController(ThongBaoService service, CauHinhThongBaoService settings) : HocTapControllerBase
{
    [HttpGet("settings")]
    public async Task<HocTapResponse<CauHinhThongBaoResponse>> CauHinh(CancellationToken ct)
    { var result = await settings.DocAsync(Chu, ct); GanPhienBan(result.PhienBan); return result; }

    [HttpPut("settings")]
    public async Task<HocTapResponse<CauHinhThongBaoResponse>> LuuCauHinh(CauHinhThongBaoRequest request, CancellationToken ct)
    { var result = await settings.LuuAsync(Chu, PhienBanYeuCau(), request, ct); GanPhienBan(result.PhienBan); return result; }

    [HttpGet]
    public Task<HopThongBaoResponse> DanhSach([FromQuery] bool chuaDoc = false, [FromQuery] int trang = 1,
        [FromQuery] int kichThuocTrang = 20, CancellationToken ct = default) => service.DanhSachAsync(Chu, chuaDoc, trang, kichThuocTrang, ct);

    [HttpPatch("{id:int:min(1)}/read")]
    public async Task<IActionResult> Doc(int id, [FromBody] DocThongBaoRequest request, CancellationToken ct)
    { await service.DocAsync(Chu, id, request.NgayPhat!.Value, ct); return NoContent(); }

    [HttpPost("read-all")]
    public async Task<IActionResult> DocTatCa([FromBody] DocTatCaThongBaoRequest request, CancellationToken ct)
    { await service.DocTatCaAsync(Chu, request.DocDenLuc!.Value, ct); return NoContent(); }
}

public sealed record DocThongBaoRequest([Required] DateTimeOffset? NgayPhat);
public sealed record DocTatCaThongBaoRequest([Required] DateTimeOffset? DocDenLuc);
