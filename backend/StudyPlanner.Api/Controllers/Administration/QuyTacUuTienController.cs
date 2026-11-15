using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.UuTien;
using StudyPlanner.Infrastructure.UuTien;

namespace StudyPlanner.Api.Controllers.Administration;

[ApiController, Authorize(Policy = "Admin"), Route("api/v1/admin/priority-rules")]
public sealed class QuyTacUuTienController(QuyTacUuTienService service) : ControllerBase
{
    [HttpGet]
    public Task<TrangQuyTacResponse> DanhSach([FromQuery] int trang = 1, [FromQuery] int kichThuocTrang = 20, CancellationToken ct = default) => service.DanhSachAsync(trang, kichThuocTrang, ct);
    [HttpPost]
    public async Task<ActionResult<QuyTacUuTienResponse>> Tao(TaoQuyTacRequest request, CancellationToken ct)
    { var result = await service.TaoAsync(request.Ten, request.CauHinh, ct); return StatusCode(201, result); }
    [HttpPost("{id:int:min(1)}/activate")]
    public Task<QuyTacUuTienResponse> ApDung(int id, ApDungQuyTacRequest request, CancellationToken ct) => service.ApDungAsync(id, request.PhienBanDangDung, ct);
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> Xoa(int id, CancellationToken ct) { await service.XoaAsync(id, ct); return NoContent(); }
}
public sealed record TaoQuyTacRequest([Required, StringLength(200)] string Ten, [Required] CauHinhUuTien CauHinh);
public sealed record ApDungQuyTacRequest([Required, Range(0, int.MaxValue)] int? PhienBanDangDung);
