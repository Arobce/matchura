using System.Text;
using ApplicationService.Application.DTOs;
using ApplicationService.Application.Interfaces;
using ApplicationService.Application.Validators;
using ApplicationService.Infrastructure.Data;
using ApplicationService.Infrastructure.Events;
using ApplicationService.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Events;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=application_db;Username=matchura_admin;Password=matchura_dev_password";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
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

// HTTP client for inter-service communication (Job Service)
builder.Services.AddHttpClient<IApplicationService, ApplicationServiceImpl>();

// RabbitMQ Event Bus
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMqEventBus>>();
    return RabbitMqEventBus.CreateAsync(config, logger).GetAwaiter().GetResult();
});

// Validators
builder.Services.AddScoped<IValidator<CreateApplicationRequest>, CreateApplicationValidator>();
builder.Services.AddScoped<IValidator<UpdateEmployerNotesRequest>, UpdateEmployerNotesValidator>();

// Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
