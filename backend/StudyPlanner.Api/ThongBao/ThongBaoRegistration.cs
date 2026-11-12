using Microsoft.EntityFrameworkCore;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.CanhBao;

namespace StudyPlanner.Api.CanhBao;

public static class ThongBaoRegistration
{
    public static IServiceCollection AddThongBao(this IServiceCollection services)
    {
        services.AddScoped<ThongBaoService>();
        services.AddHostedService<ThongBaoWorker>();
        return services;
    }
}

public sealed class ThongBaoWorker(IServiceScopeFactory scopes, ILogger<ThongBaoWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Keyset batches bound memory. Each account uses a fresh context/transaction.
                var lastId = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var ids = await db.DanhSachNguoiDung.AsNoTracking().Where(x => x.Ma > lastId && x.DangHoatDong && x.DanhSachNhiemVu.Any())
                        .OrderBy(x => x.Ma).Select(x => x.Ma).Take(100).ToListAsync(stoppingToken);
                    if (ids.Count == 0) break;
                    foreach (var id in ids)
                    {
                        try
                        {
                            using var account = scopes.CreateScope();
                            await account.ServiceProvider.GetRequiredService<ThongBaoService>().DongBoAsync(id, stoppingToken);
                        }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                        catch (Exception error) { logger.LogWarning(error, "Chưa cập nhật được thông báo cho tài khoản {Ma}; sẽ thử ở lượt sau.", id); }
                    }
                    lastId = ids[^1];
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception error) { logger.LogWarning(error, "Chưa đọc được lịch thông báo; sẽ thử ở lượt sau."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
