using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Infrastructure.Administration;

namespace StudyPlanner.Api.Controllers.Administration;

[ApiController, Authorize(Policy = "Admin"), Route("api/v1/admin/overview")]
public sealed class TongQuanAdminController(TongQuanAdminService service) : ControllerBase
{
    [HttpGet] public Task<TongQuanAdminResponse> Lay(CancellationToken ct) => service.LayAsync(ct);
}
