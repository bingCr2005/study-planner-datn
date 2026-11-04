using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed record TrangThaiTaiKhoanResponse(string ThongBao, bool DangHoatDong);

