using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

namespace StudyPlanner.Api.Controllers;

[ApiController, Route("api/v1/users/me"), Authorize]
public sealed class HoSoController(XacThucService service) : ControllerBase
{
    private int MaNguoiDungHienTai => int.Parse(User.FindFirstValue("sub")!);
    [HttpGet]
    public Task<NguoiDungResponse> XemHoSo(CancellationToken ct) => service.LayHoSoAsync(MaNguoiDungHienTai, ct);
    [HttpPatch]
    public Task<NguoiDungResponse> SuaHoSo(System.Text.Json.JsonElement request, CancellationToken ct) => service.CapNhatHoSoAsync(MaNguoiDungHienTai, request, ct);
    [HttpPost("change-password"), ProducesResponseType<DoiMatKhauResponse>(200)]
    public async Task<ActionResult<DoiMatKhauResponse>> DoiMatKhau(DoiMatKhauRequest request, CancellationToken ct)
    {
        // Chỉ báo thành công sau khi dịch vụ đã lưu và commit mật khẩu mới.
        await service.DoiMatKhauAsync(MaNguoiDungHienTai, request, ct);
        return Ok(new DoiMatKhauResponse("Đổi mật khẩu thành công. Vui lòng đăng nhập lại."));
    }
}
