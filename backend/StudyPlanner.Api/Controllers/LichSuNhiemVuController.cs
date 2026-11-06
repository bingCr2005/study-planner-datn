using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.Administration;
namespace StudyPlanner.Api.Controllers;
[Route("api/v1/task-history")]
public sealed class LichSuNhiemVuController(NhiemVuService service) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<PhanTrangResponse<LichSuNhiemVuResponse>>> DanhSach([FromQuery] DanhSachLichSuRequest q,CancellationToken ct)
    { var r=await service.LichSuAsync(Chu,q,ct); GanPhienBan(r.PhienBan); return r; }
}
