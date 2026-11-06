using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Api.Controllers;

[Route("api/v1/planning")]
public sealed class LapLichController(LapLichService service) : HocTapControllerBase
{
    [HttpPost("preview")]
    public async Task<HocTapResponse<XemTruocLichResponse>> XemTruoc(XemTruocLichRequest request, CancellationToken ct)
    {
        var result = await service.XemTruocAsync(Chu, request, ct); GanPhienBan(result.PhienBan); return result;
    }

    [HttpPost("apply")]
    public async Task<HocTapResponse<ApDungLichResponse>> ApDung(ApDungLichRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? key, CancellationToken ct)
    {
        var result = await service.ApDungAsync(Chu, key ?? "", request, ct); GanPhienBan(result.PhienBan); return result;
    }
}
