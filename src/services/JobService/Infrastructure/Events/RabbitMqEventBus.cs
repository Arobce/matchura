using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SharedKernel.Events;

namespace JobService.Infrastructure.Events;

public class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private const string ExchangeName = "matchura.events";

    private RabbitMqEventBus(IConnection connection, IChannel channel, ILogger<RabbitMqEventBus> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<RabbitMqEventBus> CreateAsync(IConfiguration configuration, ILogger<RabbitMqEventBus> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = configuration["RABBITMQ_USER"] ?? "matchura",
            Password = configuration["RABBITMQ_PASS"] ?? "matchura_dev"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

        return new RabbitMqEventBus(connection, channel, logger);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        var routingKey = typeof(T).Name;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString()
        };

        await _channel.BasicPublishAsync(ExchangeName, routingKey, false, props, body, ct);
        _logger.LogInformation("Published {EventType} to RabbitMQ", routingKey);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}
