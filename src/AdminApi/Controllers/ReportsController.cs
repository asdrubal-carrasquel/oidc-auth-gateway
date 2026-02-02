using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("reports")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetReports()
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();
        var department = Request.Headers["X-User-Department"].FirstOrDefault();

        // This endpoint requires Admin + IT + Working Hours (enforced by gateway)
        var reports = new[]
        {
            new { Id = 1, Name = "Sales Report", Type = "Monthly", GeneratedAt = DateTime.UtcNow.AddDays(-1) },
            new { Id = 2, Name = "User Activity Report", Type = "Weekly", GeneratedAt = DateTime.UtcNow.AddDays(-2) },
            new { Id = 3, Name = "System Performance Report", Type = "Daily", GeneratedAt = DateTime.UtcNow.AddHours(-6) }
        };

        return Ok(new
        {
            reports = reports,
            metadata = new
            {
                requestedBy = userName,
                department = department,
                currentHour = DateTime.UtcNow.Hour,
                timestamp = DateTime.UtcNow
            }
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetReport(int id)
    {
        return Ok(new
        {
            id = id,
            name = $"Report {id}",
            content = "Report content here...",
            generatedAt = DateTime.UtcNow
        });
    }
}
