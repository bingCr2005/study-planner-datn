using Microsoft.AspNetCore.Mvc;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet("live")]
    [ProducesResponseType(
        typeof(LivenessResponse),
        StatusCodes.Status200OK)]
    public ActionResult<LivenessResponse> Live()
    {
        var response = new LivenessResponse(
            TrangThai: "Healthy",
            KiemTraLuc: DateTimeOffset.UtcNow);

        return Ok(response);
    }
}

public record LivenessResponse(
    string TrangThai,
    DateTimeOffset KiemTraLuc);