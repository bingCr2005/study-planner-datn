using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Infrastructure.Administration;

namespace StudyPlanner.Api.Controllers.Administration;

// Chỉ khai báo GET: admin theo dõi học tập, không sửa/xóa nhiệm vụ của sinh viên.
// Policy được kiểm tra tại backend; ẩn nút trên giao diện không thay thế được nó.
// Các API nhiệm vụ/tệp/đồng hồ của sinh viên vẫn giữ nguyên kiểm tra chủ sở hữu.
[ApiController, Authorize(Policy = "Admin"), Route("api/v1/admin/learning")]
public sealed class TheoDoiHocTapController(TheoDoiHocTapService service) : ControllerBase
{
    [HttpGet("summary")]
    public Task<TongQuanHocTap> TongQuan([FromQuery] TheoDoiHocTapQuery q, CancellationToken ct) => service.TongQuanAsync(q, ct);
    [HttpGet("tasks")]
    public Task<PhanTrangResponse<NhiemVuTheoDoi>> NhiemVu([FromQuery] TheoDoiHocTapQuery q, CancellationToken ct) => service.NhiemVuAsync(q, ct);
    [HttpGet("students/{id:int:min(1)}")]
    public Task<HoSoTheoDoi> SinhVien(int id, CancellationToken ct) => service.HoSoAsync(id, ct);
    [HttpGet("students/{id:int:min(1)}/sessions")]
    public Task<PhanTrangResponse<PhienTheoDoi>> KeHoach(int id, DateTimeOffset tu, DateTimeOffset den, int trang = 1, CancellationToken ct = default) => service.KeHoachAsync(id, tu, den, trang, ct);
    [HttpGet("students/{id:int:min(1)}/focus-summary")]
    public Task<TongHopThoiGianHoc> ThoiGian(int id, DateTimeOffset tu, DateTimeOffset den, CancellationToken ct) => service.ThoiGianAsync(id, tu, den, ct);
}
