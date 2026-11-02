using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using StudyPlanner.Application.Administration;
using StudyPlanner.Infrastructure.Data;

namespace StudyPlanner.Infrastructure.Administration;

public sealed class EfAdministrationStore(ApplicationDbContext db) : IAdministrationStore
{
    public async Task<T?> FindAsync<T>(int id, CancellationToken ct) where T : class =>
        await db.Set<T>().FindAsync([id], ct);
    public Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class =>
        db.Set<T>().AnyAsync(predicate, ct);
    public Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class =>
        db.Set<T>().CountAsync(predicate, ct);
    public Task<List<T>> LoadAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken ct) where T : class =>
        db.Set<T>().Where(predicate).ToListAsync(ct);
    public async Task<PagedResult<T>> ListAsync<T>(Expression<Func<T, bool>> predicate, int page, int pageSize,
        CancellationToken ct) where T : class
    {
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
            throw new AdministrationException(FailureKind.InvalidInput, "VALIDATION_ERROR", "Trang truy vấn quá lớn.");
        var query = db.Set<T>().AsNoTracking().Where(predicate);
        var count = await query.CountAsync(ct);
        // Ma là thứ tự ổn định; không ghép tên cột/SQL do client cung cấp.
        var items = await query.OrderBy(x => EF.Property<int>(x, "Ma"))
            .Skip((int)offset).Take(pageSize).ToListAsync(ct);
        return new(items, page, pageSize, count);
    }
    public void Add<T>(T entity) where T : class => db.Set<T>().Add(entity);
    public void Remove<T>(T entity) where T : class => db.Set<T>().Remove(entity);
    public async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new AdministrationException(FailureKind.Conflict, "CONCURRENCY_CONFLICT", "Dữ liệu đã thay đổi; hãy tải lại.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && IsConflict(pg))
        {
            throw Translate((PostgresException)ex.InnerException!);
        }
    }
    public async Task<IAdministrationTransaction> BeginTransactionAsync(CancellationToken ct) =>
        new Transaction(await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct));

    // DB vẫn bảo vệ unique/FK khi hai request cùng vượt qua kiểm tra ở Application.
    public static bool IsConflict(PostgresException error) => error.SqlState is
        PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation or
        PostgresErrorCodes.CheckViolation or PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected;
    public static AdministrationException Translate(PostgresException error) => error.SqlState switch
    {
        PostgresErrorCodes.UniqueViolation => new(FailureKind.Conflict, "DUPLICATE_VALUE", "Dữ liệu bị trùng khóa duy nhất."),
        PostgresErrorCodes.ForeignKeyViolation => new(FailureKind.Conflict, "RELATED_DATA_CONFLICT", "Dữ liệu liên quan không còn tồn tại hoặc vẫn đang được sử dụng."),
        PostgresErrorCodes.CheckViolation => new(FailureKind.InvalidInput, "VALIDATION_ERROR", "Dữ liệu không thỏa ràng buộc hợp lệ."),
        _ => new(FailureKind.Conflict, "CONCURRENCY_CONFLICT", "Có cập nhật đồng thời; hãy tải lại và thử lại thao tác.")
    };
    private sealed class Transaction(IDbContextTransaction transaction) : IAdministrationTransaction
    {
        public async Task CommitAsync(CancellationToken ct)
        {
            try { await transaction.CommitAsync(ct); }
            catch (PostgresException ex) when (IsConflict(ex)) { throw Translate(ex); }
        }
        // Không commit thì DisposeAsync rollback giao dịch.
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
