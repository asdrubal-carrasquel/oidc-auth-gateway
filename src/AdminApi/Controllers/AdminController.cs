using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAdminInfo()
    {
        // Get user context from headers (propagated by gateway)
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();
        var country = Request.Headers["X-User-Country"].FirstOrDefault();
        var department = Request.Headers["X-User-Department"].FirstOrDefault();
        var tenant = Request.Headers["X-User-Tenant"].FirstOrDefault();

        return Ok(new
        {
            message = "Admin API - Access granted",
            user = new
            {
                id = userId,
                name = userName,
                country = country,
                department = department,
                tenant = tenant
            },
            systemInfo = new
            {
                status = "operational",
                version = "1.0.0",
                timestamp = DateTime.UtcNow
            }
        });
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var users = new[]
        {
            new { Id = 1, Username = "admin", Email = "admin@example.com", Role = "Admin" },
            new { Id = 2, Username = "user", Email = "user@example.com", Role = "User" },
            new { Id = 3, Username = "support", Email = "support@example.com", Role = "Support" }
        };

        return Ok(users);
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        return Ok(new
        {
            maxUsers = 1000,
            features = new[] { "RBAC", "ABAC", "OAuth2", "OIDC" },
            maintenanceMode = false
        });
    }
}
