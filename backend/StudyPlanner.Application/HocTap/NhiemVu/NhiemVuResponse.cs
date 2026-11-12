using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;
public sealed record NhiemVuResponse(int Ma, int? MaHocKy, int? MaMonHoc, int? MaNhiemVuCha,
    string TieuDe, string? MoTa, LoaiNhiemVu Loai, MucDoUuTienNhiemVu MucDoUuTien,
    DateTimeOffset? HanHoanThanh, DateTimeOffset? HanHieuLuc, int? MaNhiemVuNguonHan,
    DateTimeOffset? BatDauSuKien, DateTimeOffset? KetThucSuKien,
    TrangThaiNhiemVu TrangThai, int TienDo, int? ThoiLuongDuKienPhut, int? ThoiLuongConLaiPhut,
    bool ThieuUocLuong, bool CoNhiemVuCon, bool QuaHan, int? NhacTruocGio,
    DateTimeOffset? NgayHoanThanh, DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat)
{
    public IReadOnlyList<MucChecklist> Checklist { get; init; } = [];
}
