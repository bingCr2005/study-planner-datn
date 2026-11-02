using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

// v1 nằm trong URL nghiệp vụ; /swagger/v1/swagger.json là URL tài liệu riêng.
[ApiController, Route("api/v1/auth"), AllowAnonymous, EnableRateLimiting("auth")]
public sealed class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("register"), ProducesResponseType<UserResponse>(201)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var user = await service.RegisterAsync(request, ct);
        return Created("/api/v1/users/me", user);
    }
    [HttpPost("login")]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken ct) => service.LoginAsync(request, ct);
}
