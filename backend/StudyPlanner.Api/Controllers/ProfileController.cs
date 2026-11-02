using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;
namespace StudyPlanner.Api.Controllers;
[ApiController, Route("api/users/me"), Authorize]
public sealed class ProfileController(AuthService service) : ControllerBase
{
    [HttpGet] public Task<UserResponse> Me(CancellationToken ct) => service.MeAsync(int.Parse(User.FindFirstValue("sub")!), ct);
}
