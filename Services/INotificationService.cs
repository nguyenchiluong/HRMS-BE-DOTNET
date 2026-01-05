using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

/// <summary>
/// Service interface for sending notifications via RabbitMQ
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification event to RabbitMQ
    /// </summary>
    /// <param name="notification">The notification event to send</param>
    Task SendNotificationAsync(NotificationEvent notification);
}

