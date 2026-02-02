namespace AuthGateway.ReverseProxy;

public class PathRewriteMiddleware
{
    private readonly RequestDelegate _next;

    public PathRewriteMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Rewrite paths for reverse proxy
        if (path.StartsWith("/api/orders", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Path = path.Replace("/api/orders", "", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path.Value == "/")
            {
                context.Request.Path = "/";
            }
        }
        else if (path.StartsWith("/api/admin/reports", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Path = path.Replace("/api/admin/reports", "", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path.Value == "/")
            {
                context.Request.Path = "/";
            }
        }
        else if (path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Path = path.Replace("/api/admin", "", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(context.Request.Path.Value) || context.Request.Path.Value == "/")
            {
                context.Request.Path = "/";
            }
        }

        await _next(context);
    }
}
