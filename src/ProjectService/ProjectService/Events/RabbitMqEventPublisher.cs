using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ProjectService.Events
{
    public class RabbitMqEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection? _connection;
        private readonly IModel? _channel;
        private readonly ILogger<RabbitMqEventPublisher> _logger;
        private const string ExchangeName = "project_events";

        public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
        {
            _logger = logger;
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                    Port     = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = configuration["RabbitMQ:Username"] ?? "guest",
                    Password = configuration["RabbitMQ:Password"] ?? "guest",
                    VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/"
                };

                _connection = factory.CreateConnection();
                _channel    = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("RabbitMQ connected to {Host}", factory.HostName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ unavailable – events will be logged only");
            }
        }

        public Task PublishAsync<T>(string eventType, T payload)
        {
            var message = JsonSerializer.Serialize(new
            {
                EventType   = eventType,
                OccurredAt  = DateTime.UtcNow,
                Payload     = payload
            });

            if (_channel is { IsOpen: true })
            {
                var body = Encoding.UTF8.GetBytes(message);
                var props = _channel.CreateBasicProperties();
                props.Persistent  = true;
                props.ContentType = "application/json";

                _channel.BasicPublish(
                    exchange:   ExchangeName,
                    routingKey: eventType,
                    basicProperties: props,
                    body: body);

                _logger.LogInformation("Event published: {EventType}", eventType);
            }
            else
            {
                // Fallback: log when RabbitMQ is not available
                _logger.LogWarning("Event not published (RabbitMQ offline): {EventType} | {Payload}", eventType, message);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
