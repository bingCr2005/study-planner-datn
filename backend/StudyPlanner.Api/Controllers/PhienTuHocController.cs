using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Api.Controllers;

[Route("api/v1/study-sessions")]
public sealed class PhienTuHocController(PhienTuHocService service) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<IReadOnlyList<PhienTuHocResponse>>> DanhSach(
        DateTimeOffset? tu, DateTimeOffset? den, int? maNhiemVu, int? maLanApDung, CancellationToken ct)
    {
        var result = await service.DanhSachAsync(Chu, tu, den, maNhiemVu, ct, maLanApDung); GanPhienBan(result.PhienBan); return result;
    }

    [HttpGet("{id:int}")]
    public async Task<HocTapResponse<PhienTuHocResponse>> ChiTiet(int id, CancellationToken ct)
    {
        var result = await service.ChiTietAsync(Chu, id, ct); GanPhienBan(result.PhienBan); return result;
    }

    [HttpPost]
    public async Task<ActionResult<HocTapResponse<PhienTuHocResponse>>> Them(PhienTuHocRequest request, CancellationToken ct)
    {
        var result = await service.ThemAsync(Chu, PhienBanYeuCau(), request, ct); GanPhienBan(result.PhienBan);
        return CreatedAtAction(nameof(ChiTiet), new { id = result.DuLieu.Ma }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<HocTapResponse<PhienTuHocResponse>> Sua(int id, PhienTuHocRequest request, CancellationToken ct)
    {
        var result = await service.SuaAsync(Chu, id, PhienBanYeuCau(), request, ct); GanPhienBan(result.PhienBan); return result;
    }

    [HttpPost("{id:int}/start")]
    public Task<HocTapResponse<PhienTuHocResponse>> BatDau(int id, CancellationToken ct) => Doi(id, "start", ct);
    [HttpPost("{id:int}/finish")]
    public Task<HocTapResponse<PhienTuHocResponse>> KetThuc(int id, CancellationToken ct) => Doi(id, "finish", ct);
    [HttpPost("{id:int}/miss")]
    public Task<HocTapResponse<PhienTuHocResponse>> BoLo(int id, CancellationToken ct) => Doi(id, "miss", ct);
    [HttpPost("{id:int}/cancel")]
    public Task<HocTapResponse<PhienTuHocResponse>> Huy(int id, CancellationToken ct) => Doi(id, "cancel", ct);

    private async Task<HocTapResponse<PhienTuHocResponse>> Doi(int id, string action, CancellationToken ct)
    {
        var result = await service.DoiTrangThaiAsync(Chu, id, PhienBanYeuCau(), action, ct); GanPhienBan(result.PhienBan); return result;
    }
}
