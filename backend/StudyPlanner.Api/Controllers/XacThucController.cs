using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

// v1 nằm trong URL nghiệp vụ; /swagger/v1/swagger.json là URL tài liệu riêng.
[ApiController, Route("api/v1/auth"), AllowAnonymous, EnableRateLimiting("auth")]
public sealed class XacThucController(XacThucService service) : ControllerBase
{
    [HttpPost("register"), ProducesResponseType<NguoiDungResponse>(201)]
    public async Task<ActionResult<NguoiDungResponse>> DangKy(DangKyRequest request, CancellationToken ct)
    {
        var user = await service.DangKyAsync(request, ct);
        return Created("/api/v1/users/me", user);
    }
    [HttpPost("login")]
    public Task<DangNhapResponse> DangNhap(DangNhapRequest request, CancellationToken ct) => service.DangNhapAsync(request, ct);
}
