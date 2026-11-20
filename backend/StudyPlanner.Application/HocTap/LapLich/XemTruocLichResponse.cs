namespace StudyPlanner.Application.HocTap;

public sealed record XemTruocLichResponse(IReadOnlyList<DeXuatPhienTuHoc> Phien,
    IReadOnlyList<CanhBaoLapLich> CanhBao, int TongPhutCanXep, int TongPhutDaDat,
    int TongPhutDeXuat, int TongPhutChuaXep, long PhienBanNguon, int PhienBanQuyTac,
    DateTimeOffset ThoiDiemTinh, DateTimeOffset HetHanLuc, string TokenPreview, string MuiGio)
{
    public int PhienBanUuTien { get; init; }
    public CauHinhHocResponse CauHinhHoc { get; init; } = CauHinhHocResponse.MacDinh;
    public IReadOnlyList<int> MaPhienThayThe { get; init; } = [];
    public IReadOnlyList<PhienDuKienThayThe> PhienThayThe { get; init; } = [];
}

public sealed record PhienDuKienThayThe(int Ma, int MaNhiemVu, string TieuDe,
    DateTimeOffset BatDauDuKien, DateTimeOffset KetThucDuKien);
