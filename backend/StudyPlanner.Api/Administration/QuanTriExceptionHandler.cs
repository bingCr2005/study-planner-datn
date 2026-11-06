using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyPlanner.Application.Administration;
using StudyPlanner.Infrastructure.Administration;

namespace StudyPlanner.Api.Administration;

public sealed class QuanTriExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is StudyPlanner.Application.HocTap.HocTapException hocTap)
        {
            var detail = new ProblemDetails { Status=hocTap.Status,Title=hocTap.Message,Detail=hocTap.Message,Instance=context.Request.Path };
            detail.Extensions["code"]=hocTap.Code; detail.Extensions["traceId"]=context.TraceIdentifier;
            if (hocTap.Details is not null) detail.Extensions["details"]=hocTap.Details;
            context.Response.StatusCode=hocTap.Status;
            await context.Response.WriteAsJsonAsync(detail,options:null,contentType:"application/problem+json",cancellationToken:ct);
            return true;
        }
        if (exception is StudyPlanner.Application.Auth.XacThucException)
        {
            await Auth.XacThucRegistration.WriteProblem(context, 401, "INVALID_CREDENTIALS", "Thông tin xác thực không hợp lệ.");
            return true;
        }
        var failure = exception as QuanTriException;
        // EF có thể bọc lỗi serialization trong InvalidOperationException/DbUpdateException.
        if (failure == null && exception.GetBaseException() is PostgresException pg && EfQuanLyNguoiDungStore.IsConflict(pg))
            failure = EfQuanLyNguoiDungStore.Translate(pg);

        var unavailable = exception is NpgsqlException and not PostgresException ||
            exception.GetBaseException() is NpgsqlException and not PostgresException;
        var status = failure?.LoaiLoi switch
        {
            LoaiLoiNghiepVu.InvalidInput => 400,
            LoaiLoiNghiepVu.NotFound => 404,
            LoaiLoiNghiepVu.Conflict => 409,
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
        problem.Extensions["code"] = failure?.MaLoi ?? (unavailable ? "SERVICE_UNAVAILABLE" : "INTERNAL_ERROR");
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
