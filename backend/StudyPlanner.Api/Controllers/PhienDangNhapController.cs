using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Api.Auth;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Api.Controllers;

[ApiController, Authorize, Route("api/v1/users/me/session-events")]
public sealed class PhienDangNhapController(KenhPhienDangNhap events, ApplicationDbContext db, JwtTokenIssuer tokens) : ControllerBase
{
    [HttpGet, Produces("text/event-stream")]
    public async Task TheoDoi(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue("sub")!);
        // Subscribe before the first DB read so deactivation during connection setup cannot be missed.
        using var subscription = events.Mo(id);
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-store, no-transform";
        Response.Headers["X-Accel-Buffering"] = "no";
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Also detects changes made by another API instance, expired JWTs and changed passwords.
                var user = await db.DanhSachNguoiDung.AsNoTracking().SingleOrDefaultAsync(x => x.Ma == id, ct);
                var invalid = user is not null && !user.DangHoatDong ? "account-disabled" :
                    user is null || User.FindFirstValue("pwd") != tokens.Stamp(user) ||
                    !long.TryParse(User.FindFirstValue("exp"), out var expiry) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiry
                        ? "session-invalid" : null;
                if (invalid is not null) { await Send(invalid, ct); return; }
                if (subscription.Reader.TryRead(out var queued)) { await Send(queued, ct); return; }
                await Response.WriteAsync(": heartbeat\n\n", ct);
                await Response.Body.FlushAsync(ct);
                using var heartbeat = CancellationTokenSource.CreateLinkedTokenSource(ct);
                heartbeat.CancelAfter(TimeSpan.FromSeconds(15));
                try
                {
                    var next = await subscription.Reader.ReadAsync(heartbeat.Token);
                    await Send(next, ct); return;
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }

    private async Task Send(string name, CancellationToken ct)
    {
        await Response.WriteAsync($"event: {name}\ndata: {{}}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
