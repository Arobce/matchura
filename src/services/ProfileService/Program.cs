var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/api/profiles/ping", () => Results.Ok(new { service = "ProfileService", status = "healthy" }));

app.Run();
