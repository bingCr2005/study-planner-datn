using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;
public sealed class PhienHocTap(IHocTapStore store, IHocTapTransaction tx, NguoiDung user, bool ghi) : IAsyncDisposable
{
    public NguoiDung NguoiDung { get; } = user;
    public async Task CommitAsync(bool thayDoi, CancellationToken ct)
    {
        if (thayDoi && !ghi) throw new InvalidOperationException("Không ghi trong giao dịch đọc.");
        if (thayDoi) { NguoiDung.PhienBanLapLich = checked(NguoiDung.PhienBanLapLich + 1); await store.LuuAsync(ct); }
        await tx.CommitAsync(ct);
    }
    public ValueTask DisposeAsync() => tx.DisposeAsync();
}
