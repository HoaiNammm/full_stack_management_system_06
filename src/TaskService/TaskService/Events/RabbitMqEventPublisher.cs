using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace TaskService.Events;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private IConnection? _connection;
    private IModel?      _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConfiguration _config;

    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        TryConnect();
    }

    private void TryConnect()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName    = _config["RabbitMQ:Host"]     ?? "localhost",
                Port        = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                UserName    = _config["RabbitMQ:Username"] ?? "guest",
                Password    = _config["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _config["RabbitMQ:VirtualHost"] ?? "/"
            };
            _connection = factory.CreateConnection();
            _channel    = _connection.CreateModel();

            _channel.ExchangeDeclare("task_events",    ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare("project_events", ExchangeType.Topic, durable: true);

            _logger.LogInformation("RabbitMQ connected.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("RabbitMQ unavailable — events will be skipped. {Msg}", ex.Message);
        }
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("RabbitMQ not connected — skipping event {Key}", routingKey);
            return Task.CompletedTask;
        }
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.ContentType = "application/json";
            _channel.BasicPublish(exchange, routingKey, props, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to publish event {Key}: {Msg}", routingKey, ex.Message);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
