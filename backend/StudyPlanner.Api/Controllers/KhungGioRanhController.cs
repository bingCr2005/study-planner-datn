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
    [HttpPost("batch"), ProducesResponseType(201)]
    public async Task<ActionResult<HocTapResponse<IReadOnlyList<KhungGioRanhResponse>>>> ThemNhieu(NhieuKhungGioRanhRequest request, CancellationToken ct)
    { var r = await service.ThemNhieuAsync(Chu, request, PhienBanYeuCau(), ct); GanPhienBan(r.PhienBan); return StatusCode(201, r); }
    [HttpGet("{id:int}/exceptions")]
    public async Task<HocTapResponse<IReadOnlyList<NgoaiLeGioRanhResponse>>> NgoaiLe(int id, CancellationToken ct)
    { var r = await service.NgoaiLeAsync(Chu, id, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost("{id:int}/exceptions/change-preview")]
    public async Task<HocTapResponse<AnhHuongLichResponse>> XemNgoaiLe(int id, NgoaiLeGioRanhRequest request, CancellationToken ct)
    { var r = await service.DoiNgoaiLeAsync(Chu, id, request.Ngay, request, null, true, request.Xoa, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPut("{id:int}/exceptions/{date}")]
    public async Task<HocTapResponse<AnhHuongLichResponse>> SuaNgoaiLe(int id, DateOnly date, NgoaiLeGioRanhRequest request, CancellationToken ct)
    { var r = await service.DoiNgoaiLeAsync(Chu, id, date, request, PhienBanYeuCau(), false, false, ct); GanPhienBan(r.PhienBan); return r; }
    [HttpDelete("{id:int}/exceptions/{date}"), ProducesResponseType(204)]
    public async Task<IActionResult> XoaNgoaiLe(int id, DateOnly date, bool huyPhienLienQuan, CancellationToken ct)
    { var r = await service.DoiNgoaiLeAsync(Chu, id, date, new() { HuyPhienLienQuan = huyPhienLienQuan }, PhienBanYeuCau(), false, true, ct); GanPhienBan(r.PhienBan); return NoContent(); }
    [HttpPut("{id}")]
    public async Task<HocTapResponse<KhungGioRanhResponse>> Sua(int id, KhungGioRanhRequest request, CancellationToken ct)
    { var r = await service.LuuAsync(Chu, id, request, PhienBanYeuCau(), ct); GanPhienBan(r.PhienBan); return r; }
    [HttpDelete("{id}"), ProducesResponseType(204)]
    public async Task<IActionResult> Xoa(int id, [FromQuery] bool huyPhienLienQuan, CancellationToken ct)
    { GanPhienBan(await service.XoaAsync(Chu, id, PhienBanYeuCau(), huyPhienLienQuan, ct)); return NoContent(); }
}
