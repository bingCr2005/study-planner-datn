using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;
public sealed record LichSuNhiemVuResponse(int Ma, int MaNhiemVu, string TieuDeNhiemVu,
    HanhDongLichSuNhiemVu HanhDong, string NguonThayDoi, string? DuLieuTruoc, string? DuLieuSau, DateTimeOffset NgayThayDoi);
