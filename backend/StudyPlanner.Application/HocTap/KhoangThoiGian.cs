namespace StudyPlanner.Application.HocTap;

/// <summary>Khoảng nửa mở [BatDau, KetThuc); hai khoảng kề nhau không giao nhau.</summary>
public sealed record KhoangThoiGian(DateTimeOffset BatDau, DateTimeOffset KetThuc);
