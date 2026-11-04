using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed class TrangThaiTaiKhoanRequest
{
    [Required] public bool? DangHoatDong { get; init; }
}

