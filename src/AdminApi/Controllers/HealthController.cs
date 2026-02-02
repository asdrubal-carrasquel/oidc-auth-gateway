using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "admin-api", timestamp = DateTime.UtcNow });
    }
}
