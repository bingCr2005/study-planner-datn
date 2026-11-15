using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Api.Controllers;

[Route("api/v1/planning-reminders")]
public sealed class NhacKeHoachController(NhacKeHoachService service) : HocTapControllerBase
{
    [HttpGet("settings")]
    public async Task<HocTapResponse<CauHinhNhacKeHoachResponse>> CauHinh(CancellationToken ct)
    { var result = await service.CauHinhAsync(Chu, ct); GanPhienBan(result.PhienBan); return result; }
    [HttpPut("settings")]
    public async Task<HocTapResponse<CauHinhNhacKeHoachResponse>> LuuCauHinh(CauHinhNhacKeHoachRequest request, CancellationToken ct)
    { var result = await service.LuuCauHinhAsync(Chu, PhienBanYeuCau(), request, ct); GanPhienBan(result.PhienBan); return result; }
    [HttpGet]
    public async Task<HocTapResponse<HopNhacKeHoachResponse>> DanhSach([FromQuery] bool chuaDoc = false,
        [FromQuery] int trang = 1, [FromQuery] int kichThuocTrang = 20, CancellationToken ct = default)
    { var result = await service.DanhSachAsync(Chu, chuaDoc, trang, kichThuocTrang, ct); GanPhienBan(result.PhienBan); return result; }
    [HttpPatch("{id:int:min(1)}/read"), HttpPut("{id:int:min(1)}/read")]
    public async Task<IActionResult> Doc(int id, [FromBody] DocNhacKeHoachRequest request, CancellationToken ct)
    { await service.DocAsync(Chu, id, request.NgayPhat!.Value, ct); return NoContent(); }
    [HttpPost("read-all")]
    public async Task<IActionResult> DocTatCa([FromBody] DocTatCaNhacKeHoachRequest request, CancellationToken ct)
    { await service.DocTatCaAsync(Chu, request.DocDenLuc!.Value, ct); return NoContent(); }
}
public sealed record DocNhacKeHoachRequest([Required] DateTimeOffset? NgayPhat);
public sealed record DocTatCaNhacKeHoachRequest([Required] DateTimeOffset? DocDenLuc);
