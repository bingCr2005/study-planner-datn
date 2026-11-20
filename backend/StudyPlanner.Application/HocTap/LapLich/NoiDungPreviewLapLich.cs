namespace StudyPlanner.Application.HocTap;

// Nội dung được máy chủ ký; không tin số phiên/phút và phiên bản nguồn do trình khách tự khai.
public sealed record NoiDungPreviewLapLich(int MaNguoiDung, long PhienBanNguon, int PhienBanQuyTac,
    DateTimeOffset ThoiDiemTinh, DateTimeOffset HetHanLuc, IReadOnlyList<ChonPhienTuHocRequest> Phien)
{
    public int PhienBanUuTien { get; init; }
    public CauHinhHocResponse CauHinhHoc { get; init; } = CauHinhHocResponse.MacDinh;
    public IReadOnlyList<int> MaPhienThayThe { get; init; } = [];
}
