using Microsoft.AspNetCore.Mvc;

namespace AuthGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "auth-gateway", timestamp = DateTime.UtcNow });
    }
}
