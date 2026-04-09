using System.Text;
using FluentValidation;
using JobService.Application.DTOs;
using JobService.Application.Interfaces;
using JobService.Application.Validators;
using JobService.Infrastructure.Data;
using JobService.Infrastructure.Events;
using JobService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Events;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=job_db;Username=matchura_admin;Password=REDACTED";

builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Authentication
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

// RabbitMQ Event Bus
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMqEventBus>>();
    return RabbitMqEventBus.CreateAsync(config, logger).GetAwaiter().GetResult();
});

// Services
builder.Services.AddScoped<IJobService, JobServiceImpl>();

// Validators
builder.Services.AddScoped<IValidator<CreateJobRequest>, CreateJobValidator>();
builder.Services.AddScoped<IValidator<UpdateJobRequest>, UpdateJobValidator>();
builder.Services.AddScoped<IValidator<CreateSkillRequest>, CreateSkillValidator>();

// Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedSkillsAsync(db);
    await DataSeeder.SeedJobsAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
