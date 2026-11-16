using Microsoft.EntityFrameworkCore;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.TepDinhKem;

namespace StudyPlanner.Api.HocTap;

/// <summary>Dọn nội dung sau xóa vĩnh viễn nhiệm vụ hoặc giao dịch tải lên thất bại.</summary>
public sealed class DonKhoTepWorker(IServiceScopeFactory scopes, KhoTepNhiemVu kho,
    TimeProvider dongHo, ILogger<DonKhoTepWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        do
        {
            try { await DonAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch { logger.LogWarning("Chưa thể dọn kho tệp; sẽ thử lại vào lượt kế tiếp."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
    public async Task DonAsync(CancellationToken ct)
    {
        if (!Directory.Exists(kho.ThuMuc)) return;
        // Chụp danh sách tệp cũ trước khi đọc DB: không xóa tệp vừa tải lên nhưng chưa COMMIT.
        var nguong = dongHo.GetUtcNow().UtcDateTime.AddHours(-24);
        var cu = Directory.EnumerateFiles(kho.ThuMuc)
            .Where(p => Guid.TryParseExact(Path.GetFileName(p), "N", out _) && File.GetLastWriteTimeUtc(p) < nguong).ToArray();
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var keys = await db.DanhSachTepDinhKemNhiemVu.IgnoreQueryFilters().Select(x => x.KhoaLuuTru).ToListAsync(ct);
        var dangDung = keys.ToHashSet(StringComparer.Ordinal);
        foreach (var path in cu)
        {
            ct.ThrowIfCancellationRequested();
            if (!dangDung.Contains(Path.GetFileName(path))) File.Delete(path);
        }
    }
}
