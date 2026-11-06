namespace StudyPlanner.Application.HocTap;
public sealed class HocTapDieuPhoi(IHocTapStore store)
{
    public async Task<PhienHocTap> MoAsync(int chu, long? expected, bool ghi, CancellationToken ct)
    {
        var tx = await store.BatDauAsync(ghi, ct);
        try
        {
            var user = await store.NguoiDungAsync(chu, ghi, ct);
            if (ghi && expected is null) throw new HocTapException(428, "PRECONDITION_REQUIRED", "Cần phiên bản dữ liệu hiện tại (If-Match).");
            if (expected.HasValue && expected.Value != user.PhienBanLapLich)
                throw new HocTapException(409, "DATA_VERSION_CONFLICT", "Dữ liệu đã thay đổi. Hãy tải lại trước khi thao tác.");
            return new PhienHocTap(store, tx, user, ghi);
        }
        catch { await tx.DisposeAsync(); throw; }
    }
}
