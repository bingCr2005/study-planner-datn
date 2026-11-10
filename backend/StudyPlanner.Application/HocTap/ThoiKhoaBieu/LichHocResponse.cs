namespace StudyPlanner.Application.HocTap;

public sealed record LichHocResponse(int Ma, int MaMonHoc, string TenMonHoc, int ThuTrongTuan,
    TimeOnly GioBatDau, TimeOnly GioKetThuc, string? PhongHoc, DateOnly? NgayApDungTu, DateOnly? NgayApDungDen,
    DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat);
