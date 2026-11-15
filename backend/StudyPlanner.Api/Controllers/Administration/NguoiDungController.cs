using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Application.Administration;

namespace StudyPlanner.Api.Controllers.Administration;

[ApiController, Authorize(Policy = "Admin"), Route("api/v1/admin/users")]
public sealed class NguoiDungController(QuanLyNguoiDungService service, KenhPhienDangNhap events) : ControllerBase
{
    [HttpGet] public Task<PhanTrangResponse<NguoiDungResponse>> DanhSach([FromQuery] DanhSachNguoiDungRequest query, CancellationToken ct) => service.LayDanhSachAsync(query, ct);
    [HttpGet("{id}")] public Task<NguoiDungResponse> ChiTiet(int id, CancellationToken ct) => service.LayChiTietAsync(id, ct);
    [HttpPost, ProducesResponseType<NguoiDungResponse>(201)]
    public async Task<ActionResult<NguoiDungResponse>> Them(TaoNguoiDungRequest request, CancellationToken ct)
    {
        var result = await service.ThemAsync(request, ct);
        return CreatedAtAction(nameof(ChiTiet), new { id = result.Ma }, result);
    }
    [HttpPut("{id}")]
    public async Task<NguoiDungResponse> Sua(int id, NguoiDungRequest request, CancellationToken ct)
    {
        var result = await service.SuaAsync(id, request, ct);
        if (!result.DangHoatDong) events.Phat(id, "account-disabled");
        return result;
    }
    [HttpDelete("{id}"), ProducesResponseType(204)]
    public async Task<IActionResult> Xoa(int id, CancellationToken ct)
    {
        await service.XoaAsync(id, ct);
        events.Phat(id, "session-invalid");
        return NoContent();
    }
    [HttpPatch("{id}/activate"), ProducesResponseType<TrangThaiTaiKhoanResponse>(200)]
    public async Task<ActionResult<TrangThaiTaiKhoanResponse>> KichHoat(int id, CancellationToken ct)
    {
        await service.CapNhatTrangThaiAsync(id, new TrangThaiTaiKhoanRequest { DangHoatDong = true }, ct);
        return Ok(new TrangThaiTaiKhoanResponse("Kích hoạt tài khoản thành công.", true));
    }
    [HttpPatch("{id}/deactivate"), ProducesResponseType<TrangThaiTaiKhoanResponse>(200)]
    public async Task<ActionResult<TrangThaiTaiKhoanResponse>> VoHieuHoa(int id, CancellationToken ct)
    {
        // Dùng chung dịch vụ để giữ giao dịch và bảo vệ Admin hoạt động cuối cùng.
        await service.CapNhatTrangThaiAsync(id, new TrangThaiTaiKhoanRequest { DangHoatDong = false }, ct);
        events.Phat(id, "account-disabled");
        return Ok(new TrangThaiTaiKhoanResponse("Vô hiệu hóa tài khoản thành công.", false));
    }
}
