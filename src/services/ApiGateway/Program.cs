var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/ping", () => Results.Ok(new { service = "ApiGateway", status = "healthy" }));

app.Run();
