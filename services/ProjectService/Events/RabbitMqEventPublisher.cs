using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ProjectService.Events;

public class RabbitMqEventPublisher : IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _isAvailable;
    private const string Exchange = "project_events";

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

public record WorkspaceCreatedEvent(Guid WorkspaceId, string Name, Guid OwnerId, DateTime OccurredAt);
public record WorkspaceMemberAddedEvent(Guid WorkspaceId, Guid UserId, string Role, DateTime OccurredAt);
public record WorkspaceMemberRemovedEvent(Guid WorkspaceId, Guid UserId, DateTime OccurredAt);
public record ProjectCreatedEvent(Guid ProjectId, Guid WorkspaceId, string Name, Guid CreatedBy, DateTime OccurredAt);
public record ProjectUpdatedEvent(Guid ProjectId, string Name, string Status, DateTime OccurredAt);
public record ProjectMemberAddedEvent(Guid ProjectId, Guid WorkspaceId, Guid UserId, string Role, DateTime OccurredAt);
public record ProjectMemberRemovedEvent(Guid ProjectId, Guid UserId, DateTime OccurredAt);
public record SprintStartedEvent(Guid SprintId, Guid ProjectId, string SprintName, string? Goal, DateTime StartDate, DateTime EndDate, DateTime OccurredAt);
public record SprintCompletedEvent(Guid SprintId, Guid ProjectId, string SprintName, DateTime OccurredAt);
