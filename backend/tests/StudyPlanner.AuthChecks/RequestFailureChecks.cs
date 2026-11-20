using System.Collections.Concurrent;
using System.Data.Common;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.Logging;
using StudyPlanner.Infrastructure.Data;

internal static class RequestFailureChecks
{
    public static async Task RunAsync()
    {
        var count = 0;
        void Check(bool value, string label) { if (!value) throw new Exception(label); count++; }
        var nested = new InvalidOperationException("EF wrapper", new NpgsqlException("Connection failed", new SocketException(10111)));
        var postgres = new PostgresException("fixture", "ERROR", "ERROR", "23505");
        Check(RequestFailure.IsDatabaseUnavailable(nested), "Nested connection failure detected");
        Check(!RequestFailure.IsDatabaseUnavailable(postgres), "SQL constraint is not connection failure");

        var logs = new ConcurrentQueue<(LogLevel Level, string Text)>();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development", Args = [] });
        builder.Configuration.Sources.Clear();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new Capture(logs));
        var connectionFailure = new FailConnection();
        builder.Services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql("Host=127.0.0.1;Database=fixture;Username=fixture")
            .AddInterceptors(connectionFailure));
        builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
        await using var services = builder.Services.BuildServiceProvider();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        DefaultHttpContext Context(bool aborted = false)
        {
            var context = new DefaultHttpContext { RequestServices = services };
            context.Request.Path = "/api/check";
            context.Request.Method = "GET";
            context.Response.Body = new MemoryStream();
            if (aborted) context.RequestAborted = new CancellationToken(true);
            return context;
        }
        foreach (var (error, expected) in new (Exception, int)[] {
            (nested, 503), (new InvalidOperationException("Application bug"), 500),
            (new OperationCanceledException("Server timeout without client abort"), 500) })
        {
            var context = Context();
            await new QuanTriExceptionHandler().TryHandleAsync(context, error, CancellationToken.None);
            Check(context.Response.StatusCode == expected, "Error status classification");
            context.Response.Body.Position = 0;
            using var body = await JsonDocument.ParseAsync(context.Response.Body);
            Check(body.RootElement.GetProperty("code").GetString() == (expected == 503 ? "SERVICE_UNAVAILABLE" : "INTERNAL_ERROR"), "Error code classification");
        }
        foreach (var error in new Exception[] { new TaskCanceledException(), nested })
        {
            var context = Context(true);
            await new ApiRequestLoggingMiddleware(_ => throw error, loggerFactory).InvokeAsync(context);
            Check(context.Response.StatusCode == 499 && context.Response.Body.Length == 0, "Cancelled request has no response body");
            Check(logs.TryDequeue(out var log) && log.Level == LogLevel.Information && log.Text.Contains("-> 499"), "Cancellation logs as information");
            context = Context(true);
            await new QuanTriExceptionHandler().TryHandleAsync(context, error, CancellationToken.None);
            Check(context.Response.StatusCode == 499 && context.Response.Body.Length == 0, "Handler avoids writing to disconnected client");
        }
        var unexpected = Context(true);
        try
        {
            await new ApiRequestLoggingMiddleware(_ => throw new InvalidOperationException("Bug"), loggerFactory).InvokeAsync(unexpected);
            throw new Exception("Unexpected exception swallowed");
        }
        catch (InvalidOperationException) { Check(logs.TryDequeue(out var log) && log.Level == LogLevel.Error && log.Text.Contains("-> 500"), "Real bug still logs error even if client aborted"); }

        var handledFailure = Context(true);
        await new ApiRequestLoggingMiddleware(context =>
        {
            context.Response.StatusCode = 500;
            return Task.CompletedTask;
        }, loggerFactory).InvokeAsync(handledFailure);
        Check(logs.TryDequeue(out var handledLog) && handledLog.Level == LogLevel.Error && handledLog.Text.Contains("-> 500"), "Handled server error is not relabelled as cancellation");

        // Gọi callback xác thực thật; interceptor chặn mở kết nối, không cần DB/socket.
        using var scope = services.CreateScope();
        var options = services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
        TokenValidatedContext Validation(bool aborted)
        {
            var context = Context(aborted);
            context.RequestServices = scope.ServiceProvider;
            return new TokenValidatedContext(context,
                new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options)
            { Principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "1")], "Bearer")) };
        }
        connectionFailure.Error = new OperationCanceledException();
        var validation = Validation(true);
        await options.Events.OnTokenValidated(validation);
        Check(validation.Result?.None == true, "Aborted authentication returns no result");
        connectionFailure.Error = nested;
        validation = Validation(true);
        await options.Events.OnTokenValidated(validation);
        Check(validation.Result?.None == true, "Aborted nested database failure returns no result");
        try { await options.Events.OnTokenValidated(Validation(false)); throw new Exception("Connection failure swallowed"); }
        catch (InvalidOperationException error) { Check(ReferenceEquals(error, nested), "Live-client database failure remains observable"); }
        var challengeContext = Context(true);
        var challenge = new JwtBearerChallengeContext(challengeContext,
            new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options, new AuthenticationProperties());
        await options.Events.OnChallenge(challenge);
        Check(challenge.Handled && challengeContext.Response.Body.Length == 0, "No challenge body for disconnected client");
        Console.WriteLine($"PASS: {count} request failure checks (nested DB errors, cancellation, logging, JWT callbacks; no database or listening port).");
    }

    private sealed class FailConnection : DbConnectionInterceptor
    {
        public Exception Error { get; set; } = new OperationCanceledException();
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection,
            ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default) => throw Error;
    }
    private sealed class Capture(ConcurrentQueue<(LogLevel, string)> entries) : ILoggerProvider
    {
        public ILogger CreateLogger(string category) => new Sink(category, entries);
        public void Dispose() { }
        private sealed class Sink(string category, ConcurrentQueue<(LogLevel, string)> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel level) => category == "API";
            public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter)
            { if (IsEnabled(level)) entries.Enqueue((level, formatter(state, error))); }
        }
    }
}
