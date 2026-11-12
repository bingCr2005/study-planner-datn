using StudyPlanner.Domain.Entities;
namespace StudyPlanner.Application.HocTap;

// Query expressions remain provider-translated; Application does not depend on EF or SQL.
public interface IHocTapStore
{
    IQueryable<T> TruyVan<T>() where T : class;
    IQueryable<NhiemVu> NhiemVuTrongThungRac();
    Task<List<T>> DanhSachAsync<T>(IQueryable<T> query, CancellationToken ct);
    Task<T?> DauTienAsync<T>(IQueryable<T> query, CancellationToken ct);
    Task<int> DemAsync<T>(IQueryable<T> query, CancellationToken ct);
    void Them<T>(T entity) where T : class;
    void Xoa<T>(T entity) where T : class;
    Task LuuAsync(CancellationToken ct);
    Task<List<NhiemVu>> CayAsync(int chu, int ma, CancellationToken ct);
    Task<IHocTapTransaction> BatDauAsync(bool ghi, CancellationToken ct);
    Task<NguoiDung> NguoiDungAsync(int chu, bool khoa, CancellationToken ct);
}
