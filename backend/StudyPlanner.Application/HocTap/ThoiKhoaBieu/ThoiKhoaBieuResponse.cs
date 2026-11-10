namespace StudyPlanner.Application.HocTap;

public sealed record ThoiKhoaBieuResponse(string MuiGio, DateTimeOffset ThoiDiemTinh, IReadOnlyList<SuKienLichResponse> SuKien);
