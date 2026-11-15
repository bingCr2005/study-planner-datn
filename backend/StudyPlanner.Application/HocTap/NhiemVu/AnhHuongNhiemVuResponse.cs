namespace StudyPlanner.Application.HocTap;
public sealed record AnhHuongNhiemVuResponse(int SoNhiemVu, int SoLichNhac, int SoThongBao, int SoTep,
    IReadOnlyList<int> MaPhienBiHuy, IReadOnlyList<int> MaPhienBiXoa, IReadOnlyList<int> MaNhiemVuThayDoi)
{
    public IReadOnlyList<int> MaNhiemVuMoLai { get; init; } = [];
    public IReadOnlyList<int> MaChecklistChuyenThanhBuoc { get; init; } = [];
    public IReadOnlyList<int> MaTienDoChuyenThanhBuoc { get; init; } = [];
}
