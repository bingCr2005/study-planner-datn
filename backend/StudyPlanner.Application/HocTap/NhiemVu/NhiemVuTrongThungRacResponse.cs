namespace StudyPlanner.Application.HocTap;

public sealed record NhiemVuTrongThungRacResponse(int Ma, string TieuDe, string? MoTa,
    DateTimeOffset NgayXoa, DateTimeOffset? HanHoanThanh, int SoNhiemVu);
