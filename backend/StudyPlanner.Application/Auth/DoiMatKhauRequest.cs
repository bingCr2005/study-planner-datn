using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;

public sealed class DoiMatKhauRequest
{
    [Required, StringLength(128)] public string MatKhauHienTai { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string MatKhauMoi { get; init; } = "";
}

