using Microsoft.AspNetCore.Mvc;

namespace OrdersApi.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "orders-api", timestamp = DateTime.UtcNow });
    }
}
