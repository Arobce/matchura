var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/api/jobs/ping", () => Results.Ok(new { service = "JobService", status = "healthy" }));

app.Run();
