using Npgsql;

namespace StudyPlanner.Api.Logging;

public static class RequestFailure
{
    public static bool IsClientCancellation(HttpContext context, Exception exception)
    {
        if (!context.RequestAborted.IsCancellationRequested) return false;
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is OperationCanceledException or IOException || current is NpgsqlException and not PostgresException)
                return true;
        return false;
    }

    public static bool IsDatabaseUnavailable(Exception exception)
    {
        // Giữ lớp Npgsql ở giữa; GetBaseException() có thể chỉ còn SocketException.
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is NpgsqlException and not PostgresException) return true;
        return false;
    }
}
