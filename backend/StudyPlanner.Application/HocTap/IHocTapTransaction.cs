namespace StudyPlanner.Application.HocTap;
public interface IHocTapTransaction : IAsyncDisposable { Task CommitAsync(CancellationToken ct); }
