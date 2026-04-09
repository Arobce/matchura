using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sentry.AspNetCore;

namespace SharedKernel;

public static class SentryExtensions
{
    public static WebApplicationBuilder AddMatchuraSentry(
        this WebApplicationBuilder builder, string serviceName)
    {
        var isDev = builder.Environment.IsDevelopment();

        builder.WebHost.UseSentry((SentryAspNetCoreOptions options) =>
        {
            options.Dsn = builder.Configuration["SENTRY_DSN"] ?? "";
            options.Environment = builder.Environment.EnvironmentName.ToLowerInvariant();
            options.Release = typeof(SentryExtensions).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "dev";
            options.ServerName = serviceName;
            options.TracesSampleRate = isDev ? 1.0 : 0.2;
            options.ProfilesSampleRate = isDev ? 1.0 : 0.1;
            options.SendDefaultPii = false;
            options.AutoSessionTracking = true;
            options.TracePropagationTargets.Add("localhost");
            options.TracePropagationTargets.Add("*.matchura.*");
            options.DefaultTags["service"] = serviceName;
        });

        return builder;
    }

    public static WebApplication UseMatchuraSentry(this WebApplication app)
    {
        app.UseSentryTracing();
        return app;
    }
}
