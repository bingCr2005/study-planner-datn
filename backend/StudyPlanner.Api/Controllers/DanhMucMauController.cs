using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Infrastructure.NenTang;

namespace StudyPlanner.Api.Controllers;

// Sinh viên chỉ đọc thông tin mẫu để tự điền form; các API ghi vẫn dành riêng cho Admin.
[ApiController, Authorize, Route("api/v1/catalog/courses")]
public sealed class DanhMucMauController(DuLieuNenTangService service) : ControllerBase
{
    [HttpGet]
    public async Task<PhanTrangResponse<GoiYMonHocResponse>> GoiY([FromQuery] string? tuKhoa,
        [FromQuery] int trang = 1, CancellationToken ct = default)
    {
        var page = await service.MonHocAsync(null, tuKhoa, trang, ct);
        return new(page.DanhSach.Select(x => new GoiYMonHocResponse(x.Ma, x.MaHocKyMau, x.TenHocKy, x.KyHieu, x.Ten, x.SoTinChi)).ToArray(),
            page.Trang, page.KichThuocTrang, page.TongSo);
    }
}

public sealed record GoiYMonHocResponse(int Ma, int MaHocKyMau, string TenHocKy, string KyHieu, string Ten, int SoTinChi);
