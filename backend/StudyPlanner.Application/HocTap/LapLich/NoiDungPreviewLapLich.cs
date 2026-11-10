namespace StudyPlanner.Application.HocTap;

// Nội dung được ký bởi server; không tin số phiên/phút và revision do client tự khai.
public sealed record NoiDungPreviewLapLich(int MaNguoiDung, long PhienBanNguon, int PhienBanQuyTac,
    DateTimeOffset ThoiDiemTinh, DateTimeOffset HetHanLuc, IReadOnlyList<ChonPhienTuHocRequest> Phien);
