using System.Diagnostics;

namespace StudyPlanner.Api.Logging;

public sealed class ApiRequestLoggingMiddleware(
    RequestDelegate next, ILoggerFactory loggerFactory)
{
    private readonly ILogger logger = loggerFactory.CreateLogger("API");

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var started = Stopwatch.GetTimestamp();
        var failed = false;
        try
        {
            await next(context);
        }
        catch (Exception error) when (RequestFailure.IsClientCancellation(context, error))
        {
            if (!context.Response.HasStarted) context.Response.StatusCode = 499;
        }
        catch
        {
            failed = true;
            throw;
        }
        finally
        {
            // Đặt ngoài exception handler để đọc mã phản hồi sau khi lỗi được xử lý.
            var cancelled = !failed && context.RequestAborted.IsCancellationRequested && context.Response.StatusCode < 500;
            var status = failed ? StatusCodes.Status500InternalServerError : cancelled ? 499 : context.Response.StatusCode;
            var level = status >= 500 ? LogLevel.Error : status >= 400 && status != 499 ? LogLevel.Warning : LogLevel.Information;
            // Chỉ ghi path; không ghi query string, header/token hoặc nội dung request.
            logger.Log(level, "{Method} {Path} -> {StatusCode} | {ElapsedMs:F0} ms",
                context.Request.Method, context.Request.Path.Value, status,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }
}
