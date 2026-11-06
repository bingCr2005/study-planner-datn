namespace StudyPlanner.Application.HocTap;

public sealed record SuKienLichResponse(string Ma, string Loai, int MaNguon, int? MaMonHoc, int? MaHocKy,
    string TieuDe, DateTimeOffset BatDau, DateTimeOffset? KetThuc, string? MauSac, string? TrangThai, bool CoTheSua);
