using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProfileService.Application.DTOs;
using ProfileService.Application.Interfaces;
using ProfileService.Application.Validators;
using ProfileService.Infrastructure.Data;
using ProfileService.Infrastructure.Services;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);
builder.AddMatchuraSentry(ServiceNames.Profile);

// Database
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL or ConnectionStrings:DefaultConnection must be configured");

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Authentication (same secret as AuthService — trusts the same tokens)
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

// Services
builder.Services.AddScoped<IProfileService, ProfileServiceImpl>();

// Validators
builder.Services.AddScoped<IValidator<CreateCandidateProfileRequest>, CreateCandidateProfileValidator>();
builder.Services.AddScoped<IValidator<UpdateCandidateProfileRequest>, UpdateCandidateProfileValidator>();
builder.Services.AddScoped<IValidator<CreateEmployerProfileRequest>, CreateEmployerProfileValidator>();
builder.Services.AddScoped<IValidator<UpdateEmployerProfileRequest>, UpdateEmployerProfileValidator>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseMatchuraSentry();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
