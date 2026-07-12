using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace NotifyService.Events;

public class RabbitMqEventPublisher : IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _isAvailable;
    private const string Exchange = "user_events";

    public RabbitMqEventPublisher(IConfiguration config, ILogger<RabbitMqEventPublisher> logger)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
                UserName = config["RabbitMQ:Username"] ?? "guest",
                Password = config["RabbitMQ:Password"] ?? "guest",
                VirtualHost = config["RabbitMQ:VirtualHost"] ?? "/"
            };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
            _isAvailable = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning("RabbitMQ unavailable, events will be skipped: {Message}", ex.Message);
            _isAvailable = false;
        }
    }

    public async Task PublishAsync<T>(string routingKey, T message)
    {
        if (!_isAvailable || _channel is null) return;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await _channel.BasicPublishAsync(Exchange, routingKey, body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
    }
}

public record UserRegisteredEvent(Guid UserId, string Name, string Email, DateTime OccurredAt);
public record UserUpdatedEvent(Guid UserId, string Name, string? AvatarUrl, DateTime OccurredAt);
