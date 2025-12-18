namespace EmployeeApi.Services;

/// <summary>
/// Service interface for publishing messages to RabbitMQ message broker
/// </summary>
public interface IMessageProducerService
{
    /// <summary>
    /// Publishes a message to the specified queue
    /// </summary>
    /// <typeparam name="T">Type of the message to publish</typeparam>
    /// <param name="message">The message object to publish</param>
    /// <param name="queueName">The name of the queue to publish to</param>
    public Task PublishMessage<T>(T message, string queueName);

    /// <summary>
    /// Publishes a message to the specified exchange with a routing key
    /// </summary>
    /// <typeparam name="T">Type of the message to publish</typeparam>
    /// <param name="message">The message object to publish</param>
    /// <param name="exchangeName">The name of the exchange</param>
    /// <param name="routingKey">The routing key for message routing</param>
    public Task PublishToExchange<T>(T message, string exchangeName, string routingKey);
}
