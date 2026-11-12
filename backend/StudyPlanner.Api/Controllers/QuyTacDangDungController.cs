using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Infrastructure.UuTien;

namespace StudyPlanner.Api.Controllers;
[ApiController, Authorize, Route("api/v1/priority-rules")]
public sealed class QuyTacDangDungController(QuyTacUuTienService service) : ControllerBase
{
    [HttpGet("active")]
    public Task<QuyTacUuTienResponse> DangDung(CancellationToken ct) => service.DangDungAsync(ct);
}
