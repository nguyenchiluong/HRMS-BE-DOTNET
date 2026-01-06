using EmployeeApi.Dtos;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services;

/// <summary>
/// Service for sending notifications via RabbitMQ
/// Publishes notification events to the hrms.exchange with routing key "notification"
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IMessageProducerService _messageProducer;
    private readonly ILogger<NotificationService> _logger;

    private const string NOTIFICATION_EXCHANGE = "hrms.exchange";
    private const string NOTIFICATION_ROUTING_KEY = "notification";

    public NotificationService(
        IMessageProducerService messageProducer,
        ILogger<NotificationService> logger)
    {
        _messageProducer = messageProducer;
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification event to RabbitMQ
    /// </summary>
    /// <param name="notification">The notification event to send</param>
    public async Task SendNotificationAsync(NotificationEvent notification)
    {
        try
        {
            await _messageProducer.PublishToExchange(
                notification,
                NOTIFICATION_EXCHANGE,
                NOTIFICATION_ROUTING_KEY
            );

            _logger.LogInformation(
                "Published notification event for employee {EmpId}: {Title}",
                notification.EmpId,
                notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish notification event for employee {EmpId}",
                notification.EmpId);
            throw;
        }
    }
}

