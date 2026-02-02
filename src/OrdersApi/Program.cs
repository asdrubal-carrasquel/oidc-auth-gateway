using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var keycloakAuthority = builder.Configuration["Keycloak:Authority"] 
    ?? "http://localhost:8080/realms/auth-gateway-realm";
var keycloakAudience = builder.Configuration["Keycloak:Audience"] 
    ?? "orders-api";

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, rolesClaim));
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
