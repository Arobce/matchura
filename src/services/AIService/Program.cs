var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/api/ai/ping", () => Results.Ok(new { service = "AIService", status = "healthy" }));

app.Run();
