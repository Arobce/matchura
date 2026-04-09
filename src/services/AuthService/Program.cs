using System.Text;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Validators;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);
builder.AddMatchuraSentry(ServiceNames.Auth);

// Database
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=auth_db;Username=matchura_admin;Password=REDACTED";

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

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

// Services
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();

// Validators
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<TwoFactorRequest>, TwoFactorRequestValidator>();
builder.Services.AddScoped<IValidator<VerifyEmailRequest>, VerifyEmailRequestValidator>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseMatchuraSentry();

// Migrate database and seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DataSeeder.SeedRolesAsync(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DataSeeder.SeedUsersAsync(userManager);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
