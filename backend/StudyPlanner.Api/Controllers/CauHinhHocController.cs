using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Api.Controllers;

[Route("api/v1/study-settings")]
public sealed class CauHinhHocController(CauHinhHocService service) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<CauHinhHocResponse>> Doc(CancellationToken ct)
    { var result = await service.DocAsync(Chu, ct); GanPhienBan(result.PhienBan); return result; }
    [HttpPut]
    public async Task<HocTapResponse<CauHinhHocResponse>> Luu(CauHinhHocRequest request, CancellationToken ct)
    { var result = await service.LuuAsync(Chu, PhienBanYeuCau(), request, ct); GanPhienBan(result.PhienBan); return result; }
}
