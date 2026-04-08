using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.Application.Interfaces;
using SharedKernel.Events;

namespace NotificationService.Infrastructure.Services;

public class RabbitMqConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqConsumerWorker> _logger;
    private const string ExchangeName = "matchura.events";

    public RabbitMqConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<RabbitMqConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for RabbitMQ to be ready
        await Task.Delay(5000, stoppingToken);

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = _configuration["RABBITMQ_USER"] ?? "matchura",
            Password = _configuration["RABBITMQ_PASS"] ?? "matchura_dev"
        };

        IConnection? connection = null;
        IChannel? channel = null;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt} failed, retrying...", attempt + 1);
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (connection == null || channel == null)
        {
            _logger.LogError("Failed to connect to RabbitMQ after 10 attempts");
            return;
        }

        // Declare exchange and queue
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        var queueResult = await channel.QueueDeclareAsync("notification-events", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        // Bind to all application events
        await channel.QueueBindAsync(queueResult.QueueName, ExchangeName, "ApplicationSubmittedEvent", cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queueResult.QueueName, ExchangeName, "ApplicationStatusChangedEvent", cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queueResult.QueueName, ExchangeName, "ApplicationWithdrawnEvent", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received event: {RoutingKey}", routingKey);

                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                switch (routingKey)
                {
                    case "ApplicationSubmittedEvent":
                        await HandleApplicationSubmitted(body, notificationService, httpClientFactory, config);
                        break;
                    case "ApplicationStatusChangedEvent":
                        await HandleApplicationStatusChanged(body, notificationService, httpClientFactory, config);
                        break;
                    case "ApplicationWithdrawnEvent":
                        await HandleApplicationWithdrawn(body, notificationService, httpClientFactory, config);
                        break;
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(queueResult.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        _logger.LogInformation("RabbitMQ consumer started, listening for notification events");

        // Keep running until cancelled
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (channel != null) await channel.CloseAsync();
            if (connection != null) await connection.CloseAsync();
        }
    }

    private async Task HandleApplicationSubmitted(string body, INotificationService svc, IHttpClientFactory httpFactory, IConfiguration config)
    {
        var evt = JsonSerializer.Deserialize<ApplicationSubmittedEvent>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt == null) return;

        var (jobTitle, employerId) = await GetJobInfoAsync(evt.JobId, httpFactory, config);

        if (!string.IsNullOrEmpty(employerId))
        {
            await svc.CreateNotificationAsync(
                employerId, "ApplicationSubmitted", "New Application",
                $"A candidate applied to your job: {jobTitle}",
                evt.ApplicationId.ToString(), "Application");
        }

        await svc.CreateNotificationAsync(
            evt.CandidateId, "ApplicationConfirmed", "Application Submitted",
            $"Your application for {jobTitle} has been submitted",
            evt.ApplicationId.ToString(), "Application");
    }

    private async Task HandleApplicationStatusChanged(string body, INotificationService svc, IHttpClientFactory httpFactory, IConfiguration config)
    {
        var evt = JsonSerializer.Deserialize<ApplicationStatusChangedEvent>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt == null) return;

        var (jobTitle, _) = await GetJobInfoAsync(evt.JobId, httpFactory, config);

        var (title, message) = evt.NewStatus switch
        {
            "Reviewed" => ("Application Reviewed", $"Your application for {jobTitle} has been reviewed"),
            "Shortlisted" => ("You're Shortlisted!", $"Great news! You've been shortlisted for {jobTitle}"),
            "Accepted" => ("Application Accepted!", $"Congratulations! You've been accepted for {jobTitle}"),
            "Rejected" => ("Application Update", $"Your application for {jobTitle} was not selected"),
            _ => ("Status Update", $"Your application status for {jobTitle} changed to {evt.NewStatus}")
        };

        await svc.CreateNotificationAsync(
            evt.CandidateId, "ApplicationStatusChanged", title, message,
            evt.ApplicationId.ToString(), "Application");
    }

    private async Task HandleApplicationWithdrawn(string body, INotificationService svc, IHttpClientFactory httpFactory, IConfiguration config)
    {
        var evt = JsonSerializer.Deserialize<ApplicationWithdrawnEvent>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt == null) return;

        var (jobTitle, employerId) = await GetJobInfoAsync(evt.JobId, httpFactory, config);

        if (!string.IsNullOrEmpty(employerId))
        {
            await svc.CreateNotificationAsync(
                employerId, "ApplicationWithdrawn", "Application Withdrawn",
                $"A candidate withdrew their application for {jobTitle}",
                evt.ApplicationId.ToString(), "Application");
        }
    }

    private async Task<(string title, string? employerId)> GetJobInfoAsync(Guid jobId, IHttpClientFactory httpFactory, IConfiguration config)
    {
        var jobServiceUrl = config["JOB_SERVICE_URL"] ?? "http://job-service:8080";
        try
        {
            var client = httpFactory.CreateClient();
            var response = await client.GetAsync($"{jobServiceUrl}/api/jobs/{jobId}");
            if (!response.IsSuccessStatusCode) return ("a job", null);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var title = doc.RootElement.GetProperty("title").GetString() ?? "a job";
            var employerId = doc.RootElement.GetProperty("employerId").GetString();
            return (title, employerId);
        }
        catch
        {
            return ("a job", null);
        }
    }
}
