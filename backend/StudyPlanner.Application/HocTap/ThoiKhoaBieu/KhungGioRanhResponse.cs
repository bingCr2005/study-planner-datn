namespace StudyPlanner.Application.HocTap;

public sealed record KhungGioRanhResponse(int Ma, int ThuTrongTuan, TimeOnly GioBatDau, TimeOnly GioKetThuc,
    DateOnly NgayBatDau, DateOnly NgayKetThuc, bool DangSuDung, DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat);
