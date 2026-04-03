var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/api/applications/ping", () => Results.Ok(new { service = "ApplicationService", status = "healthy" }));

app.Run();
