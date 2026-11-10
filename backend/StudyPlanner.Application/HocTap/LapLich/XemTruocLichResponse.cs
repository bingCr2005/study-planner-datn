namespace StudyPlanner.Application.HocTap;

public sealed record XemTruocLichResponse(IReadOnlyList<DeXuatPhienTuHoc> Phien,
    IReadOnlyList<CanhBaoLapLich> CanhBao, int TongPhutCanXep, int TongPhutDaDat,
    int TongPhutDeXuat, int TongPhutChuaXep, long PhienBanNguon, int PhienBanQuyTac,
    DateTimeOffset ThoiDiemTinh, DateTimeOffset HetHanLuc, string TokenPreview, string MuiGio);
