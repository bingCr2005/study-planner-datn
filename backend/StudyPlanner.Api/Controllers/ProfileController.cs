using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

[ApiController, Route("api/v1/users/me"), Authorize]
public sealed class ProfileController(AuthService service) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue("sub")!);
    [HttpGet]
    public Task<UserResponse> Me(CancellationToken ct) => service.MeAsync(UserId, ct);
    [HttpPatch]
    public Task<UserResponse> Update(System.Text.Json.JsonElement request, CancellationToken ct) => service.UpdateAsync(UserId, request, ct);
    [HttpPost("change-password"), ProducesResponseType(204)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    { await service.ChangePasswordAsync(UserId, request, ct); return NoContent(); }
}
