using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskService.Services;

namespace TaskService.Events;

public class ProjectEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory            _scopeFactory;
    private readonly IConfiguration                  _config;
    private readonly ILogger<ProjectEventConsumer>   _logger;
    private IConnection? _connection;
    private IModel?      _channel;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProjectEventConsumer(
        IServiceScopeFactory          scopeFactory,
        IConfiguration                config,
        ILogger<ProjectEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _config       = config;
        _logger       = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(Cleanup);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName    = _config["RabbitMQ:Host"]        ?? "localhost",
                Port        = int.Parse(_config["RabbitMQ:Port"]    ?? "5672"),
                UserName    = _config["RabbitMQ:Username"]    ?? "guest",
                Password    = _config["RabbitMQ:Password"]    ?? "guest",
                VirtualHost = _config["RabbitMQ:VirtualHost"] ?? "/"
            };

            _connection = factory.CreateConnection();
            _channel    = _connection.CreateModel();

            _channel.ExchangeDeclare("project_events", ExchangeType.Topic, durable: true);

            const string queue = "taskservice.project_events";
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue, "project_events", "project.created");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    await HandleAsync(body);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing project.created event");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            _logger.LogInformation("ProjectEventConsumer started — listening to project.created");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ unavailable — ProjectEventConsumer not started");
        }

        return Task.CompletedTask;
    }

    private async Task HandleAsync(string body)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope<ProjectCreatedPayload>>(body, _jsonOpts);
        var payload  = envelope?.Payload;
        if (payload == null) return;

        using var scope        = _scopeFactory.CreateScope();
        var       kanbanService = scope.ServiceProvider.GetRequiredService<KanbanService>();

        var columns = payload.Columns
            .OrderBy(c => c.Position)
            .Select(c => (c.Name, c.Type, c.Position));

        await kanbanService.SeedColumnsFromEventAsync(payload.ProjectId, columns);

        _logger.LogInformation(
            "Seeded {Count} Kanban columns for project {ProjectId} (template: {Template})",
            payload.Columns.Count, payload.ProjectId, payload.TemplateId);
    }

    private void Cleanup()
    {
        _channel?.Close();
        _connection?.Close();
    }

    public override void Dispose()
    {
        Cleanup();
        base.Dispose();
    }

    // ── Private DTOs (mirrors ProjectService EventMessages) ──────────

    private class EventEnvelope<T>
    {
        public string?   EventType  { get; set; }
        public DateTime  OccurredAt { get; set; }
        public T?        Payload    { get; set; }
    }

    private class ProjectCreatedPayload
    {
        public Guid               ProjectId  { get; set; }
        public string             TemplateId { get; set; } = string.Empty;
        public List<ColumnDto>    Columns    { get; set; } = new();
    }

    private class ColumnDto
    {
        [JsonPropertyName("name")]     public string Name     { get; set; } = string.Empty;
        [JsonPropertyName("type")]     public string Type     { get; set; } = string.Empty;
        [JsonPropertyName("position")] public int    Position { get; set; }
    }
}
