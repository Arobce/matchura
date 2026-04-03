var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/api/auth/ping", () => Results.Ok(new { service = "AuthService", status = "healthy" }));

app.Run();
