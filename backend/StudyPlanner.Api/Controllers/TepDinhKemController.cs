using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Infrastructure.TepDinhKem;

namespace StudyPlanner.Api.Controllers;

[Route("api/v1/tasks/{id:int}/attachments")]
public sealed class TepDinhKemController(TepDinhKemService service) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<IReadOnlyList<TepDinhKemResponse>>> DanhSach(int id, CancellationToken ct)
    { var r = await service.DanhSachAsync(Chu, id, ct); GanPhienBan(r.PhienBan); return r; }

    [HttpPost, RequestSizeLimit(KhoTepNhiemVu.GioiHanByte + 65536), RequestFormLimits(MultipartBodyLengthLimit = KhoTepNhiemVu.GioiHanByte + 65536)]
    public async Task<ActionResult<HocTapResponse<TepDinhKemResponse>>> Them(int id, IFormFile tep, CancellationToken ct)
    {
        await using var stream = tep.OpenReadStream();
        var r = await service.ThemAsync(Chu, id, PhienBanYeuCau(), tep.FileName, stream, tep.Length, ct);
        GanPhienBan(r.PhienBan);
        return CreatedAtAction(nameof(TaiXuong), new { id, tepId = r.DuLieu.Ma }, r);
    }
    [HttpGet("{tepId:int}/download")]
    public async Task<IActionResult> TaiXuong(int id, int tepId, CancellationToken ct)
    {
        var r = await service.TaiAsync(Chu, id, tepId, ct);
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        // Luôn tải xuống, không thực thi HTML/SVG do người dùng tải lên trong nguồn của ứng dụng.
        return File(r.NoiDung, "application/octet-stream", r.Ten);
    }
    [HttpDelete("{tepId:int}")]
    public async Task<IActionResult> Xoa(int id, int tepId, CancellationToken ct)
    { GanPhienBan(await service.XoaAsync(Chu, id, tepId, PhienBanYeuCau(), ct)); return NoContent(); }
}
