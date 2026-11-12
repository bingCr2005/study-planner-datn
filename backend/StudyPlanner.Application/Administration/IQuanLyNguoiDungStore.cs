using System.Linq.Expressions;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Administration;

// Chỉ là cổng lưu trữ cho nhóm quản trị. Application không tham chiếu EF Core/Npgsql.
public interface IQuanLyNguoiDungStore
{
    Task<T?> FindAsync<T>(int id, CancellationToken ct) where T : class;
    Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class;
    Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class;
    Task<PhanTrangResponse<T>> ListAsync<T>(Expression<Func<T, bool>> predicate, int page, int pageSize,
        CancellationToken ct) where T : class;
    Task<List<T>> LoadAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken ct, bool includeFiltered = false) where T : class;
    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    Task SaveAsync(CancellationToken ct);
    Task<IQuanLyNguoiDungTransaction> BeginTransactionAsync(CancellationToken ct);
}

