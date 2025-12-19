using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EmployeeApi.Services;

/// <summary>
/// Service for publishing messages to RabbitMQ message broker
/// </summary>
public class MessageProducerService : IMessageProducerService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<MessageProducerService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    public MessageProducerService(IConnection connection, ILogger<MessageProducerService> logger)
    {
        _connection = connection;
        _logger = logger;
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Publishes a message to the specified queue
    /// </summary>
    public async Task PublishMessage<T>(T message, string queueName)
    {
        try
        {
            // Declare the queue (creates it if it doesn't exist)
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var jsonMessage = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Message published to queue {QueueName}: {Message}", queueName, jsonMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to queue {QueueName}", queueName);
            throw;
        }
    }

    /// <summary>
    /// Publishes a message to the specified exchange with a routing key
    /// </summary>
    public async Task PublishToExchange<T>(T message, string exchangeName, string routingKey)
    {
        try
        {
            // Declare the exchange (creates it if it doesn't exist)
            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            var jsonMessage = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation(
                "Message published to exchange {ExchangeName} with routing key {RoutingKey}: {Message}",
                exchangeName, routingKey, jsonMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to exchange {ExchangeName}", exchangeName);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
