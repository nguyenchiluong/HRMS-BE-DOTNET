namespace EmployeeApi.Dtos;

/// <summary>
/// Notification event model for publishing to RabbitMQ
/// Matches the Spring Boot consumer's NotificationEvent DTO
/// </summary>
public class NotificationEvent
{
    /// <summary>
    /// Employee ID to receive the notification
    /// </summary>
    public long EmpId { get; set; }

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Notification message content
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Optional notification type: "info", "warning", "success", "error"
    /// </summary>
    public string? Type { get; set; }
}

