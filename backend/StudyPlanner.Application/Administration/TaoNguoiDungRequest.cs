using System.ComponentModel.DataAnnotations;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Administration;

public sealed class TaoNguoiDungRequest : NguoiDungRequest
{
    [Required, StringLength(128, MinimumLength = 8)] public string MatKhau { get; init; } = "";
}

