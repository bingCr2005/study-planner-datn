using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.Administration;
namespace StudyPlanner.Api.Controllers;
[Route("api/v1/tasks")]
public sealed class NhiemVuController(NhiemVuService service) : HocTapControllerBase
{
    [HttpGet]
    public async Task<HocTapResponse<PhanTrangResponse<NhiemVuResponse>>> DanhSach([FromQuery] DanhSachNhiemVuRequest q,CancellationToken ct)
    { var r = await service.DanhSachAsync(Chu,q,ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("{id:int}")]
    public async Task<HocTapResponse<NhiemVuResponse>> ChiTiet(int id,CancellationToken ct)
    { var r = await service.ChiTietAsync(Chu,id,ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("{id:int}/tree")]
    public async Task<HocTapResponse<IReadOnlyList<NhiemVuResponse>>> Cay(int id,CancellationToken ct)
    { var r = await service.CayAsync(Chu,id,ct); GanPhienBan(r.PhienBan); return r; }
    [HttpPost,ProducesResponseType(201)]
    public async Task<ActionResult<HocTapResponse<NhiemVuResponse>>> Them(NhiemVuRequest request,CancellationToken ct)
    { var r = await service.ThemAsync(Chu,PhienBanYeuCau(),request,ct); GanPhienBan(r.PhienBan); return CreatedAtAction(nameof(ChiTiet),new{id=r.DuLieu.Ma},r); }
    [HttpPut("{id:int}")]
    public Task<HocTapResponse<NhiemVuResponse>> Sua(int id,NhiemVuRequest request,CancellationToken ct) => DoiNhiemVu(id,"update",request,ct);
    [HttpPost("{id:int}/children"),ProducesResponseType(201)]
    public async Task<ActionResult<HocTapResponse<IReadOnlyList<NhiemVuResponse>>>> ThemCon(int id,ThemNhiemVuConRequest request,CancellationToken ct)
    {
        var r=await Doi(id,"children",request,ct);
        var result=new HocTapResponse<IReadOnlyList<NhiemVuResponse>>(r.PhienBan,(IReadOnlyList<NhiemVuResponse>)r.DuLieu);
        return CreatedAtAction(nameof(Cay),new{id},result);
    }
    [HttpPatch("{id:int}/parent")]
    public Task<HocTapResponse<NhiemVuResponse>> DoiCha(int id,ChaNhiemVuRequest request,CancellationToken ct) => DoiNhiemVu(id,"parent",request,ct);
    [HttpPatch("{id:int}/scope")]
    public Task<HocTapResponse<NhiemVuResponse>> DoiPhamVi(int id,PhamViNhiemVuRequest request,CancellationToken ct) => DoiNhiemVu(id,"scope",request,ct);
    [HttpPatch("{id:int}/progress")]
    public Task<HocTapResponse<NhiemVuResponse>> TienDo(int id,TienDoNhiemVuRequest request,CancellationToken ct) => DoiNhiemVu(id,"progress",request,ct);
    [HttpPost("{id:int}/complete")]
    public Task<HocTapResponse<NhiemVuResponse>> HoanThanh(int id,TienDoNhiemVuRequest request,CancellationToken ct) => DoiNhiemVu(id,"complete",request,ct);
    [HttpPost("{id:int}/reopen")]
    public Task<HocTapResponse<NhiemVuResponse>> MoLai(int id,TienDoNhiemVuRequest request,CancellationToken ct) => DoiNhiemVu(id,"reopen",request,ct);
    [HttpPost("{id:int}/change-preview")]
    public async Task<HocTapResponse<AnhHuongNhiemVuResponse>> XemThayDoi(int id,XemThayDoiNhiemVuRequest request,CancellationToken ct)
    { var r = await service.XemTruocAsync(Chu,id,request,ct); GanPhienBan(r.PhienBan); return r; }
    [HttpGet("{id:int}/deletion-preview")]
    public Task<HocTapResponse<AnhHuongNhiemVuResponse>> XemXoa(int id,CancellationToken ct) => XemThayDoi(id,new XemThayDoiNhiemVuRequest {ThaoTac="delete"},ct);
    [HttpDelete("{id:int}"),ProducesResponseType(204)]
    public async Task<IActionResult> Xoa(int id,bool huyPhienLienQuan,CancellationToken ct)
    { var r = await Doi(id,"delete",new TienDoNhiemVuRequest {HuyPhienLienQuan=huyPhienLienQuan},ct); GanPhienBan(r.PhienBan); return NoContent(); }
    private async Task<HocTapResponse<object>> Doi(int id,string action,object request,CancellationToken ct)
    { var r = await service.ThayDoiAsync(Chu,id,PhienBanYeuCau(),action,request,false,ct); GanPhienBan(r.PhienBan); return r; }
    private async Task<HocTapResponse<NhiemVuResponse>> DoiNhiemVu(int id,string action,object request,CancellationToken ct)
    { var r=await Doi(id,action,request,ct); return new(r.PhienBan,(NhiemVuResponse)r.DuLieu); }
}
