using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Configuration;
using AuthGateway.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var keycloakAuthority = builder.Configuration["Keycloak:Authority"] 
    ?? "http://localhost:8080/realms/auth-gateway-realm";
var keycloakAudience = builder.Configuration["Keycloak:Audience"] 
    ?? "auth-gateway-api";

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = false; // Set to true in production
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
        
        // Map roles from token
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    // Extract roles from resource_access or realm_access
                    var resourceAccess = context.Principal?.FindFirst("resource_access")?.Value;
                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        try
                        {
                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            if (resourceAccessJson.RootElement.TryGetProperty(keycloakAudience, out var audienceElement))
                            {
                                if (audienceElement.TryGetProperty("roles", out var rolesElement))
                                {
                                    foreach (var role in rolesElement.EnumerateArray())
                                    {
                                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.GetString() ?? string.Empty));
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    // Extract realm roles
                    var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccess))
                    {
                        try
                        {
                            var realmAccessJson = JsonDocument.Parse(realmAccess);
                            if (realmAccessJson.RootElement.TryGetProperty("roles", out var rolesElement))
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.GetString() ?? string.Empty));
                                }
                            }
                        }
                        catch { }
                    }

                    // Map roles claim if present
                    var rolesClaim = context.Principal?.FindFirst("roles")?.Value;
                    if (!string.IsNullOrEmpty(rolesClaim))
                    {
                        try
                        {
                            var rolesArray = JsonDocument.Parse(rolesClaim);
                            foreach (var role in rolesArray.RootElement.EnumerateArray())
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.GetString() ?? string.Empty));
                            }
                        }
                        catch
                        {
                            // If not JSON array, treat as single role
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, rolesClaim));
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// Authorization Policies - RBAC + ABAC
builder.Services.AddAuthorization(options =>
{
    // RBAC Policies
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUser", policy => policy.RequireRole("User", "Admin", "Support"));
    
    // ABAC Policies
    options.AddPolicy("RequireChile", policy => 
        policy.RequireClaim("country", "CL"));
    
    options.AddPolicy("RequireITDepartment", policy => 
        policy.RequireClaim("department", "IT"));
    
    options.AddPolicy("RequireTenant", policy => 
        policy.RequireAssertion(ctx => 
            ctx.User.HasClaim(c => c.Type == "tenant")));
    
    // Combined RBAC + ABAC Policies
    options.AddPolicy("UserChile", policy =>
    {
        policy.RequireRole("User", "Admin", "Support");
        policy.RequireClaim("country", "CL");
    });
    
    options.AddPolicy("AdminChileIT", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("country", "CL");
        policy.RequireClaim("department", "IT");
    });
    
    options.AddPolicy("AdminWorkingHours", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireAssertion(ctx =>
        {
            var hour = DateTime.UtcNow.Hour;
            // Working hours: 12:00 - 22:00 UTC (adjust as needed)
            return hour >= 12 && hour <= 22;
        });
    });
    
    options.AddPolicy("AdminITWorkingHours", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("department", "IT");
        policy.RequireAssertion(ctx =>
        {
            var hour = DateTime.UtcNow.Hour;
            return hour >= 12 && hour <= 22;
        });
    });
});

// YARP Reverse Proxy Configuration
var ordersApiUrl = builder.Configuration["OrdersApi:BaseUrl"] ?? "http://localhost:5001";
var adminApiUrl = builder.Configuration["AdminApi:BaseUrl"] ?? "http://localhost:5002";

builder.Services.AddSingleton<IProxyConfigProvider>(sp => 
    new ReverseProxyConfigProvider(ordersApiUrl, adminApiUrl));

builder.Services.AddReverseProxy();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must be the FIRST middleware (before anything else)
app.UseCors("AllowAngular");

// Handle preflight OPTIONS requests explicitly (before authentication)
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        await context.Response.WriteAsync(string.Empty);
        return;
    }
    await next();
});

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Add middleware to propagate user context and rewrite paths
app.UseMiddleware<PathRewriteMiddleware>();
app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();
app.MapReverseProxy();

app.Run();
