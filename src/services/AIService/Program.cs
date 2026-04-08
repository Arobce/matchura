using System.Text;
using System.Threading.Channels;
using AIService.Agents;
using AIService.Agents.Core;
using AIService.Application.DTOs;
using AIService.Application.Interfaces;
using AIService.Application.Validators;
using AIService.Infrastructure.BackgroundJobs;
using AIService.Infrastructure.Data;
using AIService.Infrastructure.Events;
using AIService.Infrastructure.Services;
using AIService.Infrastructure.TextExtraction;
using SharedKernel.Events;
using Amazon;
using Amazon.S3;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=ai_db;Username=matchura_admin;Password=REDACTED";

builder.Services.AddDbContext<AIDbContext>(options =>
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

// Redis
var redisConnection = builder.Configuration["REDIS_CONNECTION"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// AWS S3
var awsRegion = builder.Configuration["AWS_REGION"] ?? "us-east-1";
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
    builder.Configuration["AWS_ACCESS_KEY_ID"],
    builder.Configuration["AWS_SECRET_ACCESS_KEY"],
    RegionEndpoint.GetBySystemName(awsRegion)));
builder.Services.AddScoped<IS3StorageService, S3StorageService>();

// Claude API client
builder.Services.AddHttpClient<ClaudeApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    var apiKey = builder.Configuration["ANTHROPIC_API_KEY"]
        ?? throw new InvalidOperationException("ANTHROPIC_API_KEY is not configured");
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    client.Timeout = TimeSpan.FromMinutes(2);
});

// AI Agents
builder.Services.AddScoped<ResumeParserAgent>();
builder.Services.AddScoped<JobMatcherAgent>();
builder.Services.AddScoped<SkillGapAnalyzerAgent>();

// Text extractors
builder.Services.AddSingleton<ITextExtractor, PdfTextExtractor>();
builder.Services.AddSingleton<ITextExtractor, DocxTextExtractor>();

// RabbitMQ Event Bus
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMqEventBus>>();
    return RabbitMqEventBus.CreateAsync(config, logger).GetAwaiter().GetResult();
});

// Resume parsing background worker channel
builder.Services.AddSingleton(Channel.CreateUnbounded<Guid>());
builder.Services.AddHostedService<ResumeParsingWorker>();

// Job matching background worker (auto-match on job publish)
builder.Services.AddHostedService<JobMatchingWorker>();

// Services
builder.Services.AddScoped<IResumeService, ResumeServiceImpl>();
builder.Services.AddHttpClient<IMatchingService, MatchingServiceImpl>();
builder.Services.AddHttpClient<ISkillGapService, SkillGapServiceImpl>();
builder.Services.AddHttpClient<IAnalyticsService, AnalyticsServiceImpl>();

// Validators
builder.Services.AddScoped<IValidator<ComputeMatchRequest>, ComputeMatchRequestValidator>();
builder.Services.AddScoped<IValidator<AnalyzeSkillGapRequest>, AnalyzeSkillGapRequestValidator>();

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
    var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/api/ai/ping", () => Results.Ok(new { service = "AIService", status = "healthy" }));

app.Run();
