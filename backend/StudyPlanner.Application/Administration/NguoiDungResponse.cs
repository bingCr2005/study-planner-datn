using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

// DTO không có MatKhauBam hoặc navigation để tránh lộ thông tin và vòng JSON.
public sealed record NguoiDungResponse(int Ma, string Email, string HoTen, string? MaSoSinhVien,
    string? TenTruong, string? TenLop, string? NganhHoc, VaiTroNguoiDung VaiTro, bool DangHoatDong, string MuiGio, int NhacTruocGio,
    DateTimeOffset NgayTao, DateTimeOffset NgayCapNhat);

