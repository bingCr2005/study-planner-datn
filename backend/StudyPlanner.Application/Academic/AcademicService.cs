using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Application.Academic;

public sealed class AcademicService(IAdministrationStore store, TimeProvider clock)
{
    private static AdministrationException Missing() => new(FailureKind.NotFound, "RESOURCE_NOT_FOUND", "Không tìm thấy học kỳ/môn học.");
    private static void ValidateId(int id)
    {
        if (id <= 0) throw new AdministrationException(FailureKind.InvalidInput, "VALIDATION_ERROR", "ID phải lớn hơn 0.");
    }
    private async Task<Semester> SemesterAsync(int userId, int id, CancellationToken ct)
    {
        ValidateId(id);
        return (await store.LoadAsync<Semester>(x => x.Ma == id && x.MaNguoiDung == userId, ct)).SingleOrDefault() ?? throw Missing();
    }
    private async Task<Course> CourseAsync(int userId, int id, CancellationToken ct)
    {
        ValidateId(id);
        return (await store.LoadAsync<Course>(x => x.Ma == id && x.Semester.MaNguoiDung == userId, ct)).SingleOrDefault() ?? throw Missing();
    }
    public async Task<PagedResult<SemesterResponse>> ListSemestersAsync(int userId, AcademicListRequest query, CancellationToken ct)
    {
        ResourceRules.Validate(query);
        var keyword = query.TuKhoa?.Trim().ToUpperInvariant() ?? "";
        return ResourceRules.Map(await store.ListAsync<Semester>(x => x.MaNguoiDung == userId &&
            (keyword == "" || x.Ten.ToUpper().Contains(keyword)), query.Trang, query.KichThuocTrang, ct), Dto);
    }
    public async Task<SemesterResponse> GetSemesterAsync(int userId, int id, CancellationToken ct) => Dto(await SemesterAsync(userId, id, ct));
    public async Task<SemesterResponse> CreateSemesterAsync(int userId, SemesterRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        var now = clock.GetUtcNow();
        var semester = new Semester { MaNguoiDung = userId, NgayTao = now, NgayCapNhat = now };
        Apply(semester, request);
        store.Add(semester); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return Dto(semester);
    }
    public async Task<SemesterResponse> UpdateSemesterAsync(int userId, int id, SemesterRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        var semester = await SemesterAsync(userId, id, ct);
        Apply(semester, request); semester.NgayCapNhat = clock.GetUtcNow();
        await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return Dto(semester);
    }
    public async Task<PagedResult<CourseResponse>> ListCoursesAsync(int userId, int semesterId, AcademicListRequest query, CancellationToken ct)
    {
        ResourceRules.Validate(query);
        await SemesterAsync(userId, semesterId, ct);
        var keyword = query.TuKhoa?.Trim().ToUpperInvariant() ?? "";
        return ResourceRules.Map(await store.ListAsync<Course>(x => x.MaHocKy == semesterId && x.Semester.MaNguoiDung == userId &&
            (keyword == "" || x.Ten.ToUpper().Contains(keyword) || (x.KyHieu != null && x.KyHieu.ToUpper().Contains(keyword))),
            query.Trang, query.KichThuocTrang, ct), Dto);
    }
    public async Task<CourseResponse> GetCourseAsync(int userId, int id, CancellationToken ct) => Dto(await CourseAsync(userId, id, ct));
    public async Task<CourseResponse> CreateCourseAsync(int userId, int semesterId, CourseRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        await SemesterAsync(userId, semesterId, ct);
        var now = clock.GetUtcNow();
        var course = new Course { MaHocKy = semesterId, NgayTao = now, NgayCapNhat = now };
        Apply(course, request);
        store.Add(course); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return Dto(course);
    }
    public async Task<CourseResponse> UpdateCourseAsync(int userId, int id, CourseRequest request, CancellationToken ct)
    {
        ResourceRules.Validate(request);
        await using var transaction = await store.BeginTransactionAsync(ct);
        var course = await CourseAsync(userId, id, ct);
        Apply(course, request); course.NgayCapNhat = clock.GetUtcNow();
        await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return Dto(course);
    }
    public async Task<DeletionPreviewResponse> PreviewSemesterAsync(int userId, int id, CancellationToken ct)
    {
        await SemesterAsync(userId, id, ct);
        var courseIds = (await store.LoadAsync<Course>(x => x.MaHocKy == id, ct)).Select(x => x.Ma).ToArray();
        return new(courseIds.Length, await store.CountAsync<ClassSession>(x => courseIds.Contains(x.MaMonHoc), ct),
            await store.CountAsync<TaskItem>(x => x.MaNguoiDung == userId && x.MaHocKy == id, ct));
    }
    public async Task<DeletionPreviewResponse> PreviewCourseAsync(int userId, int id, CancellationToken ct)
    {
        await CourseAsync(userId, id, ct);
        return new(1, await store.CountAsync<ClassSession>(x => x.MaMonHoc == id, ct),
            await store.CountAsync<TaskItem>(x => x.MaNguoiDung == userId && x.MaMonHoc == id, ct));
    }
    public async Task DeleteSemesterAsync(int userId, int id, CancellationToken ct)
    {
        await using var transaction = await store.BeginTransactionAsync(ct);
        var semester = await SemesterAsync(userId, id, ct);
        var courses = await store.LoadAsync<Course>(x => x.MaHocKy == id, ct);
        var courseIds = courses.Select(x => x.Ma).ToArray();
        var tasks = await store.LoadAsync<TaskItem>(x => x.MaHocKy == id || (x.MaMonHoc.HasValue && courseIds.Contains(x.MaMonHoc.Value)), ct);
        DetachTasks(userId, tasks, removeSemester: true);
        // Persist history and FK changes before deleting principals. Any failure rolls back all writes.
        await store.SaveAsync(ct);
        foreach (var session in await store.LoadAsync<ClassSession>(x => courseIds.Contains(x.MaMonHoc), ct)) store.Remove(session);
        foreach (var course in courses) store.Remove(course);
        store.Remove(semester); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
    }
    public async Task DeleteCourseAsync(int userId, int id, CancellationToken ct)
    {
        await using var transaction = await store.BeginTransactionAsync(ct);
        var course = await CourseAsync(userId, id, ct);
        DetachTasks(userId, await store.LoadAsync<TaskItem>(x => x.MaMonHoc == id, ct), removeSemester: false);
        await store.SaveAsync(ct);
        foreach (var session in await store.LoadAsync<ClassSession>(x => x.MaMonHoc == id, ct)) store.Remove(session);
        store.Remove(course); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
    }
    private void DetachTasks(int userId, List<TaskItem> tasks, bool removeSemester)
    {
        if (tasks.Any(x => x.MaNguoiDung != userId))
            throw ResourceRules.Conflict("INCONSISTENT_TASK_OWNER", "Nhiệm vụ liên quan có chủ sở hữu không nhất quán.");
        var now = clock.GetUtcNow();
        foreach (var task in tasks)
        {
            var before = Links(task);
            task.MaMonHoc = null; task.Course = null;
            if (removeSemester) { task.MaHocKy = null; task.Semester = null; }
            task.NgayCapNhat = now;
            store.Add(new TaskHistory { MaNguoiDung = userId, MaNhiemVu = task.Ma, TieuDeNhiemVu = task.TieuDe,
                HanhDong = TaskHistoryAction.Updated, NgayThayDoi = now, DuLieuTruoc = before, DuLieuSau = Links(task) });
        }
    }
    private static string Links(TaskItem task) => JsonSerializer.Serialize(new { task.MaMonHoc, task.MaHocKy }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    private static void Apply(Semester value, SemesterRequest request)
    {
        value.Ten = request.Ten.Trim(); value.NgayBatDau = request.NgayBatDau!.Value; value.NgayKetThuc = request.NgayKetThuc!.Value;
    }
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Apply(Course value, CourseRequest request)
    {
        value.Ten = request.Ten.Trim(); value.KyHieu = Optional(request.KyHieu); value.SoTinChi = request.SoTinChi;
        value.GiangVien = Optional(request.GiangVien); value.MauSac = Optional(request.MauSac)?.ToUpperInvariant();
    }
    private static SemesterResponse Dto(Semester x) => new(x.Ma, x.Ten, x.NgayBatDau, x.NgayKetThuc, x.NgayTao, x.NgayCapNhat);
    private static CourseResponse Dto(Course x) => new(x.Ma, x.MaHocKy, x.Ten, x.KyHieu, x.SoTinChi, x.GiangVien, x.MauSac, x.NgayTao, x.NgayCapNhat);
}
