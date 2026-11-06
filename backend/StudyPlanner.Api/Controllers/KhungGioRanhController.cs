using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
namespace StudyPlanner.Api.Controllers;

[Route("api/v1/availability-windows")]
public sealed class KhungGioRanhController(KhungGioRanhService service) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<IReadOnlyList<KhungGioRanhResponse>>> DanhSach(CancellationToken ct)
    { var r = await service.DanhSachAsync(Chu, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("{id}")]
    public async Task<HocTapResponse<KhungGioRanhResponse>> ChiTiet(int id, CancellationToken ct)
    { var r = await service.ChiTietAsync(Chu, id, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost, ProducesResponseType(201)]
    public async Task<ActionResult<HocTapResponse<KhungGioRanhResponse>>> Them(KhungGioRanhRequest request, CancellationToken ct)
    { var r = await service.LuuAsync(Chu, null, request, PhienBanYeuCau(), ct); GanPhienBan(r.PhienBan); return CreatedAtAction(nameof(ChiTiet), new { id = r.DuLieu.Ma }, r); }
    [HttpPut("{id}")]
    public async Task<HocTapResponse<KhungGioRanhResponse>> Sua(int id, KhungGioRanhRequest request, CancellationToken ct)
    { var r = await service.LuuAsync(Chu, id, request, PhienBanYeuCau(), ct); GanPhienBan(r.PhienBan); return r; }
    [HttpDelete("{id}"), ProducesResponseType(204)]
    public async Task<IActionResult> Xoa(int id, [FromQuery] bool huyPhienLienQuan, CancellationToken ct)
    { GanPhienBan(await service.XoaAsync(Chu, id, PhienBanYeuCau(), huyPhienLienQuan, ct)); return NoContent(); }
}
