using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Academic;
using StudyPlanner.Application.Administration;

namespace StudyPlanner.Api.Controllers;

[ApiController, Authorize, Route("api/v1/semesters")]
[ProducesResponseType<ProblemDetails>(400), ProducesResponseType<ProblemDetails>(401), ProducesResponseType<ProblemDetails>(404)]
public sealed class SemestersController(AcademicService service) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue("sub")!);
    [HttpGet] public Task<PagedResult<SemesterResponse>> List([FromQuery] AcademicListRequest query, CancellationToken ct) => service.ListSemestersAsync(UserId, query, ct);
    [HttpGet("{id}")] public Task<SemesterResponse> Get(int id, CancellationToken ct) => service.GetSemesterAsync(UserId, id, ct);
    [HttpPost, ProducesResponseType<SemesterResponse>(201), ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<SemesterResponse>> Create(SemesterRequest request, CancellationToken ct)
    {
        var result = await service.CreateSemesterAsync(UserId, request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Ma }, result);
    }
    [HttpPut("{id}"), ProducesResponseType<ProblemDetails>(409)]
    public Task<SemesterResponse> Update(int id, SemesterRequest request, CancellationToken ct) => service.UpdateSemesterAsync(UserId, id, request, ct);
    [HttpGet("{id}/deletion-preview")]
    public Task<DeletionPreviewResponse> Preview(int id, CancellationToken ct) => service.PreviewSemesterAsync(UserId, id, ct);
    [HttpDelete("{id}"), ProducesResponseType(204), ProducesResponseType<ProblemDetails>(409)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await service.DeleteSemesterAsync(UserId, id, ct); return NoContent(); }
    [HttpGet("{id}/courses")]
    public Task<PagedResult<CourseResponse>> ListCourses(int id, [FromQuery] AcademicListRequest query, CancellationToken ct) => service.ListCoursesAsync(UserId, id, query, ct);
    [HttpPost("{id}/courses"), ProducesResponseType<CourseResponse>(201), ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<CourseResponse>> CreateCourse(int id, CourseRequest request, CancellationToken ct)
    {
        var result = await service.CreateCourseAsync(UserId, id, request, ct);
        return CreatedAtAction(nameof(CoursesController.Get), "Courses", new { id = result.Ma }, result);
    }
}
