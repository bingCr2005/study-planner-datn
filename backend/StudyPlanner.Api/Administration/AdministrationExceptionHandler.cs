using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyPlanner.Application.Administration;
using StudyPlanner.Infrastructure.Administration;

namespace StudyPlanner.Api.Administration;

public sealed class AdministrationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is StudyPlanner.Application.Auth.AuthenticationFailureException)
        {
            await Auth.AuthRegistration.WriteProblem(context, 401, "INVALID_CREDENTIALS", "Thông tin xác thực không hợp lệ.");
            return true;
        }
        var failure = exception as AdministrationException;
        // EF có thể bọc lỗi serialization trong InvalidOperationException/DbUpdateException.
        if (failure == null && exception.GetBaseException() is PostgresException pg && EfAdministrationStore.IsConflict(pg))
            failure = EfAdministrationStore.Translate(pg);

        var unavailable = exception is NpgsqlException and not PostgresException ||
            exception.GetBaseException() is NpgsqlException and not PostgresException;
        var status = failure?.Kind switch
        {
            FailureKind.InvalidInput => 400,
            FailureKind.NotFound => 404,
            FailureKind.Conflict => 409,
            _ => unavailable ? 503 : 500
        };
        var problem = new ProblemDetails
        {
            Status = status, Title = status switch
            {
                400 => "Dữ liệu không hợp lệ", 404 => "Không tìm thấy dữ liệu", 409 => "Xung đột dữ liệu",
                503 => "Dịch vụ dữ liệu chưa sẵn sàng", _ => "Lỗi xử lý yêu cầu"
            },
            Detail = failure?.Message ?? "Không thể hoàn tất yêu cầu. Vui lòng thử lại sau.",
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = failure?.Code ?? (unavailable ? "SERVICE_UNAVAILABLE" : "INTERNAL_ERROR");
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
