using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Academic;

namespace StudyPlanner.Api.Controllers;

[ApiController, Authorize, Route("api/v1/courses")]
[ProducesResponseType<ProblemDetails>(400), ProducesResponseType<ProblemDetails>(401), ProducesResponseType<ProblemDetails>(404)]
public sealed class CoursesController(AcademicService service) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue("sub")!);
    [HttpGet("{id}")] public Task<CourseResponse> Get(int id, CancellationToken ct) => service.GetCourseAsync(UserId, id, ct);
    [HttpPut("{id}"), ProducesResponseType<ProblemDetails>(409)]
    public Task<CourseResponse> Update(int id, CourseRequest request, CancellationToken ct) => service.UpdateCourseAsync(UserId, id, request, ct);
    [HttpGet("{id}/deletion-preview")]
    public Task<DeletionPreviewResponse> Preview(int id, CancellationToken ct) => service.PreviewCourseAsync(UserId, id, ct);
    [HttpDelete("{id}"), ProducesResponseType(204), ProducesResponseType<ProblemDetails>(409)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await service.DeleteCourseAsync(UserId, id, ct); return NoContent(); }
}
