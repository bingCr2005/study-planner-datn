using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.NenTang;
using StudyPlanner.Infrastructure.NenTang;

namespace StudyPlanner.Api.Controllers.Administration;

[ApiController, Authorize(Policy = "Admin"), Route("api/v1/admin/catalog")]
public sealed class DuLieuNenTangController(DuLieuNenTangService service) : ControllerBase
{
    [HttpGet("semesters")]
    public Task<IReadOnlyList<HocKyMauResponse>> HocKy(CancellationToken ct) => service.HocKyAsync(ct);
    [HttpPost("semesters")]
    public async Task<IActionResult> TaoHocKy(HocKyMauRequest request, CancellationToken ct) => StatusCode(201, await service.LuuHocKyAsync(null, request, ct));
    [HttpPut("semesters/{id:int:min(1)}")]
    public Task<HocKyMauResponse> SuaHocKy(int id, HocKyMauRequest request, CancellationToken ct) => service.LuuHocKyAsync(id, request, ct);
    [HttpDelete("semesters/{id:int:min(1)}")]
    public async Task<IActionResult> XoaHocKy(int id, [FromQuery] int? phienBan, CancellationToken ct) { await service.XoaAsync(true, id, phienBan, ct); return NoContent(); }
    [HttpGet("courses")]
    public Task<Application.Administration.PhanTrangResponse<MonHocMauResponse>> MonHoc([FromQuery] int? maHocKyMau, [FromQuery] string? tuKhoa, [FromQuery] int trang = 1, CancellationToken ct = default) => service.MonHocAsync(maHocKyMau, tuKhoa, trang, ct);
    [HttpPost("courses")]
    public async Task<IActionResult> TaoMon(MonHocMauRequest request, CancellationToken ct) => StatusCode(201, await service.LuuMonHocAsync(null, request, ct));
    [HttpPut("courses/{id:int:min(1)}")]
    public Task<MonHocMauResponse> SuaMon(int id, MonHocMauRequest request, CancellationToken ct) => service.LuuMonHocAsync(id, request, ct);
    [HttpDelete("courses/{id:int:min(1)}")]
    public async Task<IActionResult> XoaMon(int id, [FromQuery] int? phienBan, CancellationToken ct) { await service.XoaAsync(false, id, phienBan, ct); return NoContent(); }
}
