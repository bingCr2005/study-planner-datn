
namespace StudyPlanner.Application.Administration;

public sealed class QuanTriException(LoaiLoiNghiepVu kind, string code, string message)
    : Exception(message)
{
    public LoaiLoiNghiepVu LoaiLoi { get; } = kind;
    public string MaLoi { get; } = code;
}

