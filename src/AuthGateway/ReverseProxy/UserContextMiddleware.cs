using System.Security.Claims;

namespace AuthGateway.ReverseProxy;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // Propagate user context to downstream services
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? context.User.FindFirst("sub")?.Value;
            var userName = context.User.FindFirst("preferred_username")?.Value;
            var country = context.User.FindFirst("country")?.Value;
            var department = context.User.FindFirst("department")?.Value;
            var tenant = context.User.FindFirst("tenant")?.Value;

            if (!string.IsNullOrEmpty(userId))
                context.Request.Headers["X-User-Id"] = userId;
            
            if (!string.IsNullOrEmpty(userName))
                context.Request.Headers["X-User-Name"] = userName;
            
            if (!string.IsNullOrEmpty(country))
                context.Request.Headers["X-User-Country"] = country;
            
            if (!string.IsNullOrEmpty(department))
                context.Request.Headers["X-User-Department"] = department;
            
            if (!string.IsNullOrEmpty(tenant))
                context.Request.Headers["X-User-Tenant"] = tenant;
        }

        await _next(context);
    }
}
