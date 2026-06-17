using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotifyService.Api.Services;

namespace NotifyService.Api.Events;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration,
        ILogger<RabbitMqConsumer> logger)
    {
        _scopeFactory  = scopeFactory;
        _configuration = configuration;
        _logger        = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(Cleanup);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName    = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port        = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName    = _configuration["RabbitMQ:Username"] ?? "guest",
                Password    = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/"
            };

            _connection = factory.CreateConnection();
            _channel    = _connection.CreateModel();

            // Declare exchanges (idempotent — safe if already declared by other services)
            _channel.ExchangeDeclare("task_events",    ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare("project_events", ExchangeType.Topic, durable: true);

            BindAndConsume("notify.task_events",    "task_events",
                new[] { "task.column.changed", "task.assigned" });

            BindAndConsume("notify.project_events", "project_events",
                new[] { "project.member.added" });

            _logger.LogInformation("RabbitMQ consumer started");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ unavailable — consumer not started");
        }

        return Task.CompletedTask;
    }

    private void BindAndConsume(string queueName, string exchange, string[] routingKeys)
    {
        _channel!.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

        foreach (var key in routingKeys)
            _channel.QueueBind(queueName, exchange, key);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var body      = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;

            try
            {
                await HandleMessageAsync(routingKey, body);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {RoutingKey}", routingKey);
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    private async Task HandleMessageAsync(string routingKey, string body)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        switch (routingKey)
        {
            case "task.column.changed":
                var columnEvent = Deserialize<EventEnvelope<TaskColumnChangedPayload>>(body);
                if (columnEvent?.Payload?.AssignedTo is Guid assignee)
                {
                    await notificationService.CreateAsync(
                        userId:           assignee,
                        title:            $"Task \"{columnEvent.Payload.TaskTitle}\" đã được chuyển cột",
                        content:          $"Task được chuyển sang column mới (type: {columnEvent.Payload.NewColumnType})",
                        type:             "task_column_changed",
                        relatedTaskId:    columnEvent.Payload.TaskId,
                        relatedProjectId: columnEvent.Payload.ProjectId);
                }
                break;

            case "task.assigned":
                var assignEvent = Deserialize<EventEnvelope<TaskAssignedPayload>>(body);
                if (assignEvent?.Payload?.NewAssignee is Guid newAssignee)
                {
                    await notificationService.CreateAsync(
                        userId:           newAssignee,
                        title:            $"Bạn được giao task \"{assignEvent.Payload.TaskTitle}\"",
                        content:          "Một task mới vừa được phân công cho bạn.",
                        type:             "task_assigned",
                        relatedTaskId:    assignEvent.Payload.TaskId,
                        relatedProjectId: assignEvent.Payload.ProjectId);
                }
                break;

            case "project.member.added":
                var memberEvent = Deserialize<EventEnvelope<MemberAddedPayload>>(body);
                if (memberEvent?.Payload is { } mp)
                {
                    await notificationService.CreateAsync(
                        userId:           mp.UserId,
                        title:            "Bạn đã được thêm vào dự án",
                        content:          $"Bạn đã tham gia dự án với vai trò {RoleName(mp.Role)}.",
                        type:             "member_added",
                        relatedProjectId: mp.ProjectId);
                }
                break;

            default:
                _logger.LogWarning("Unknown routing key: {Key}", routingKey);
                break;
        }
    }

    private static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, _jsonOptions);

    private static string RoleName(int role) => role switch
    {
        0 => "Owner",
        1 => "Manager",
        2 => "Member",
        3 => "Viewer",
        _ => "Member"
    };

    private void Cleanup()
    {
        try
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
            }
        }
        catch
        {
            // Ignore cleanup error
        }

        try
        {
            if (_connection != null && _connection.IsOpen)
            {
                _connection.Close();
            }
        }
        catch
        {
            // Ignore cleanup error
        }
    }

    public override void Dispose()
    {
        Cleanup();
        base.Dispose();
    }
}