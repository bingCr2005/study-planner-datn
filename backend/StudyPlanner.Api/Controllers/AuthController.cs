using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

// Điểm vào đăng nhập; đăng ký được bổ sung ở bước tiếp theo.
[ApiController, Route("api/auth"), AllowAnonymous, EnableRateLimiting("auth")]
public sealed class AuthController(AuthService service) : ControllerBase
{

    [HttpPost("login")]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken ct) => service.LoginAsync(request, ct);
}
