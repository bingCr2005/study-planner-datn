namespace StudyPlanner.Application.Administration;

public enum FailureKind { InvalidInput, NotFound, Conflict }

public sealed class AdministrationException(FailureKind kind, string code, string message)
    : Exception(message)
{
    public FailureKind Kind { get; } = kind;
    public string Code { get; } = code;
}
