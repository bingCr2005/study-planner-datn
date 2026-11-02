using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

// Đăng ký và đăng nhập sinh viên.
[ApiController, Route("api/auth"), AllowAnonymous, EnableRateLimiting("auth")]
public sealed class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("register"), ProducesResponseType<UserResponse>(201)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var user = await service.RegisterAsync(request, ct);
        return Created("/api/users/me", user);
    }
    [HttpPost("login")]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken ct) => service.LoginAsync(request, ct);
}
