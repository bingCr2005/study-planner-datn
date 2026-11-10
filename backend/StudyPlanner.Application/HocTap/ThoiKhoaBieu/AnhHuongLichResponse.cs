namespace StudyPlanner.Application.HocTap;

public sealed record AnhHuongLichResponse(IReadOnlyList<int> PhienBiAnhHuong, IReadOnlyList<int> LichHocTrung, bool CanKetThucPhienDangHoc);
