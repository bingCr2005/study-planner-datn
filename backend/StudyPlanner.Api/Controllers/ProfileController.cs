using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

[ApiController, Route("api/users/me"), Authorize]
public sealed class ProfileController(AuthService service) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue("sub")!);
    [HttpGet]
    public Task<UserResponse> Me(CancellationToken ct) => service.MeAsync(UserId, ct);
    [HttpPatch]
    public Task<UserResponse> Update(System.Text.Json.JsonElement request, CancellationToken ct) => service.UpdateAsync(UserId, request, ct);
    [HttpPost("change-password"), ProducesResponseType<ChangePasswordResponse>(200)]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        // Chỉ báo thành công sau khi dịch vụ đã lưu và commit mật khẩu mới.
        await service.ChangePasswordAsync(UserId, request, ct);
        return Ok(new ChangePasswordResponse("Đổi mật khẩu thành công. Vui lòng đăng nhập lại."));
    }
}
