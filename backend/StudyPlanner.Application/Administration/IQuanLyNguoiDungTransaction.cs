using System.Linq.Expressions;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Administration;

public interface IQuanLyNguoiDungTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}

