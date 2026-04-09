using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.TestUtilities.Fakes;
using SharedKernel.Events;
using Testcontainers.PostgreSql;
using Xunit;

namespace Shared.TestUtilities;

public class CustomWebApplicationFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    private const string TestJwtSecret = "integration-test-secret-key-that-is-long-enough-for-hmac-sha256";
    private const string TestJwtIssuer = "matchura";
    private const string TestJwtAudience = "matchura-clients";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public FakeEventBus EventBus { get; } = new();

    private readonly Action<IServiceCollection>? _configureServices;

    public CustomWebApplicationFactory(Action<IServiceCollection>? configureServices = null)
    {
        _configureServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("JWT_SECRET", TestJwtSecret);
        builder.UseSetting("JWT_ISSUER", TestJwtIssuer);
        builder.UseSetting("JWT_AUDIENCE", TestJwtAudience);

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            // Add DbContext with Testcontainers PostgreSQL
            services.AddDbContext<TDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Replace IEventBus with FakeEventBus
            var eventBusDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEventBus));
            if (eventBusDescriptor != null) services.Remove(eventBusDescriptor);
            services.AddSingleton<IEventBus>(EventBus);

            _configureServices?.Invoke(services);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }

    public static string GenerateJwtToken(
        string userId,
        string role,
        string? name = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name ?? $"Test {role}")
        };

        var token = new JwtSecurityToken(
            issuer: TestJwtIssuer,
            audience: TestJwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public HttpClient CreateAuthenticatedClient(string userId, string role, string? name = null)
    {
        var client = CreateClient();
        var token = GenerateJwtToken(userId, role, name);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        EventBus.Clear();
    }
}
