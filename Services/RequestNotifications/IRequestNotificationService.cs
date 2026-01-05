namespace EmployeeApi.Services.RequestNotifications;

/// <summary>
/// Service for sending email notifications for request approval/rejection
/// </summary>
public interface IRequestNotificationService
{
    /// <summary>
    /// Sends approval email notification for profile and time-off requests
    /// </summary>
    Task SendApprovalEmailAsync(Models.Request request, string? comment);

    /// <summary>
    /// Sends rejection email notification for profile and time-off requests
    /// </summary>
    Task SendRejectionEmailAsync(Models.Request request, string reason);
}

