using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Administration;
using StudyPlanner.Application.Administration;

namespace StudyPlanner.Api.Controllers.Administration;

[ApiController, Authorize(Policy = "Admin"), Route("api/admin/users")]
public sealed class UsersController(UserAdministrationService service) : ControllerBase
{
    [HttpGet] public Task<PagedResult<UserResponse>> List([FromQuery] ListRequest query, CancellationToken ct) => service.ListAsync(query, ct);
    [HttpGet("{id}")] public Task<UserResponse> Get(int id, CancellationToken ct) => service.GetAsync(id, ct);
    [HttpPost, ProducesResponseType<UserResponse>(201)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Ma }, result);
    }
    [HttpPut("{id}")] public Task<UserResponse> Update(int id, UserRequest request, CancellationToken ct) => service.UpdateAsync(id, request, ct);
    [HttpDelete("{id}"), ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await service.DeleteAsync(id, ct); return NoContent(); }
}
