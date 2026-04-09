using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);
builder.AddMatchuraSentry(ServiceNames.Gateway);

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// JWT Authentication (validate at gateway before forwarding)
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? "matchura",
            ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? "matchura-clients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// CORS — allow Next.js frontend
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(',')
    ?? new[] { "http://localhost:3000", "http://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    // Anonymous endpoints: 30 requests/minute
    options.AddFixedWindowLimiter("anonymous", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // Authenticated endpoints: 100 requests/minute
    options.AddFixedWindowLimiter("authenticated", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    // AI endpoints: 20 requests/minute (LLM API is expensive)
    options.AddFixedWindowLimiter("ai", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Health checks for downstream services — reads YARP cluster addresses so prod overrides apply automatically
var reverseProxyConfig = builder.Configuration.GetSection("ReverseProxy:Clusters");
var healthChecks = builder.Services.AddHealthChecks();

var clusterServiceMap = new Dictionary<string, string>
{
    ["auth-cluster"] = "auth-service",
    ["profile-cluster"] = "profile-service",
    ["job-cluster"] = "job-service",
    ["application-cluster"] = "application-service",
    ["ai-cluster"] = "ai-service",
    ["notification-cluster"] = "notification-service"
};

foreach (var (cluster, serviceName) in clusterServiceMap)
{
    var address = reverseProxyConfig[$"{cluster}:Destinations:{serviceName}:Address"];
    if (address != null)
    {
        healthChecks.AddUrlGroup(new Uri($"{address}/health"), name: serviceName, tags: ["downstream"]);
    }
}

var app = builder.Build();
app.UseMatchuraSentry();

// Request logging middleware
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    await next();
    var duration = DateTime.UtcNow - start;
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Gateway");
    logger.LogInformation("{Method} {Path} → {StatusCode} ({Duration}ms)",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        (int)duration.TotalMilliseconds);
});

app.UseCors();
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapReverseProxy();
app.MapHealthChecks("/health");
app.MapGet("/ping", () => Results.Ok(new { service = "ApiGateway", status = "healthy" }));

app.Run();
