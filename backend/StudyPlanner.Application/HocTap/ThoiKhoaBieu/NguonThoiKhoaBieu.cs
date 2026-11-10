using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;

/// <summary>Dữ liệu toàn tài khoản; bộ lọc môn của giao diện không được làm mất khoảng bận khi lập lịch.</summary>
public sealed record NguonThoiKhoaBieu(NguoiDung NguoiDung, IReadOnlyList<KhoangThoiGian> LichHoc,
    IReadOnlyList<KhoangThoiGian> GioRanh, IReadOnlyList<KhoangThoiGian> SuKien, IReadOnlyList<PhienTuHoc> PhienTuHoc);
