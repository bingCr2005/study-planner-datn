using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
namespace StudyPlanner.Api.Controllers;

[Route("api/v1/class-sessions")]
public sealed class LichHocController(LichHocService service) : HocTapControllerBase
{
    [HttpGet("/api/v1/courses/{id}/class-sessions")]
    public async Task<HocTapResponse<IReadOnlyList<LichHocResponse>>> DanhSach(int id, CancellationToken ct)
    { var r = await service.DanhSachAsync(Chu, id, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("{id}")]
    public async Task<HocTapResponse<LichHocResponse>> ChiTiet(int id, CancellationToken ct)
    { var r = await service.ChiTietAsync(Chu, id, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost("/api/v1/courses/{id}/class-sessions"), ProducesResponseType(201)]
    public async Task<ActionResult<HocTapResponse<LichHocResponse>>> Them(int id, LichHocRequest request, CancellationToken ct)
    { var r = await service.LuuAsync(Chu, null, id, request, PhienBanYeuCau(), ct); GanPhienBan(r.PhienBan); return CreatedAtAction(nameof(ChiTiet), new { id = r.DuLieu.Ma }, r); }
    [HttpPut("{id}")]
    public async Task<HocTapResponse<LichHocResponse>> Sua(int id, LichHocRequest request, CancellationToken ct)
    { var r = await service.LuuAsync(Chu, id, null, request, PhienBanYeuCau(), ct); GanPhienBan(r.PhienBan); return r; }
    [HttpDelete("{id}"), ProducesResponseType(204)]
    public async Task<IActionResult> Xoa(int id, CancellationToken ct)
    { GanPhienBan(await service.XoaAsync(Chu, id, PhienBanYeuCau(), ct)); return NoContent(); }
}
