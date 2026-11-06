namespace StudyPlanner.Application.HocTap;
public sealed class HocTapException(int status, string code, string message, object? details = null) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public object? Details { get; } = details;
}
