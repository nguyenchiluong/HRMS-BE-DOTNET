using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services.RequestNotifications;

public class RequestNotificationService : IRequestNotificationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMessageProducerService _messageProducer;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILogger<RequestNotificationService> _logger;

    private const string SEND_EMAIL_QUEUE = "sendEmail";

    public RequestNotificationService(
        IEmployeeRepository employeeRepository,
        IMessageProducerService messageProducer,
        IEmailTemplateService emailTemplateService,
        ILogger<RequestNotificationService> logger)
    {
        _employeeRepository = employeeRepository;
        _messageProducer = messageProducer;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public async Task SendApprovalEmailAsync(Models.Request request, string? comment)
    {
        try
        {
            var requestTypeCode = request.RequestTypeLookup?.Code?.ToUpper() ?? "";
            var category = request.RequestTypeLookup?.Category?.ToLower() ?? "";

            // Only send emails for profile and time-off requests (not timesheet)
            if (category != "profile" && category != "time-off")
            {
                return;
            }

            // Get requester personal email
            var requester = request.Requester ?? await _employeeRepository.GetByIdAsync(request.RequesterEmployeeId);
            if (requester == null || string.IsNullOrWhiteSpace(requester.PersonalEmail))
            {
                _logger.LogWarning("Cannot send approval email: requester not found or personal email missing for request {RequestId}", request.Id);
                return;
            }

            // Get approver name
            var approver = request.Approver ?? await _employeeRepository.GetByIdAsync(request.ApproverEmployeeId ?? 0);
            var approverName = approver?.FullName ?? "HR Team";

            // Format request type name
            var requestTypeName = FormatRequestTypeName(requestTypeCode);

            // Get date/duration info for time-off requests
            string? startDate = null;
            string? endDate = null;
            int? duration = null;
            if (category == "time-off" && request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue)
            {
                startDate = request.EffectiveFrom.Value.ToString("MMMM dd, yyyy");
                endDate = request.EffectiveTo.Value.ToString("MMMM dd, yyyy");
                duration = CalculateDuration(request.EffectiveFrom.Value, request.EffectiveTo.Value);
            }

            var emailData = new RequestNotificationEmailData(
                EmployeeName: requester.FullName,
                RequestType: requestTypeName,
                RequestId: request.Id.ToString(),
                StartDate: startDate,
                EndDate: endDate,
                Duration: duration,
                Comment: comment,
                RejectionReason: null,
                ApproverName: approverName
            );

            var emailEvent = new SendEmailEvent
            {
                EmailToSend = requester.PersonalEmail!,
                Subject = $"Your {requestTypeName} Request Has Been Approved",
                HtmlContent = _emailTemplateService.GenerateRequestApprovalEmail(emailData)
            };

            await _messageProducer.PublishMessage(emailEvent, SEND_EMAIL_QUEUE);
            _logger.LogInformation(
                "Published approval email event for request {RequestId} to personal email {Email}",
                request.Id, requester.PersonalEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send approval email for request {RequestId}",
                request.Id);
            // Don't throw - email sending failure shouldn't fail the approval
        }
    }

    public async Task SendRejectionEmailAsync(Models.Request request, string reason)
    {
        try
        {
            var requestTypeCode = request.RequestTypeLookup?.Code?.ToUpper() ?? "";
            var category = request.RequestTypeLookup?.Category?.ToLower() ?? "";

            // Only send emails for profile and time-off requests (not timesheet)
            if (category != "profile" && category != "time-off")
            {
                return;
            }

            // Get requester personal email
            var requester = request.Requester ?? await _employeeRepository.GetByIdAsync(request.RequesterEmployeeId);
            if (requester == null || string.IsNullOrWhiteSpace(requester.PersonalEmail))
            {
                _logger.LogWarning("Cannot send rejection email: requester not found or personal email missing for request {RequestId}", request.Id);
                return;
            }

            // Get approver name
            var approver = request.Approver ?? await _employeeRepository.GetByIdAsync(request.ApproverEmployeeId ?? 0);
            var approverName = approver?.FullName ?? "HR Team";

            // Format request type name
            var requestTypeName = FormatRequestTypeName(requestTypeCode);

            // Get date/duration info for time-off requests
            string? startDate = null;
            string? endDate = null;
            int? duration = null;
            if (category == "time-off" && request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue)
            {
                startDate = request.EffectiveFrom.Value.ToString("MMMM dd, yyyy");
                endDate = request.EffectiveTo.Value.ToString("MMMM dd, yyyy");
                duration = CalculateDuration(request.EffectiveFrom.Value, request.EffectiveTo.Value);
            }

            var emailData = new RequestNotificationEmailData(
                EmployeeName: requester.FullName,
                RequestType: requestTypeName,
                RequestId: request.Id.ToString(),
                StartDate: startDate,
                EndDate: endDate,
                Duration: duration,
                Comment: null,
                RejectionReason: reason,
                ApproverName: approverName
            );

            var emailEvent = new SendEmailEvent
            {
                EmailToSend = requester.PersonalEmail!,
                Subject = $"Your {requestTypeName} Request Has Been Rejected",
                HtmlContent = _emailTemplateService.GenerateRequestRejectionEmail(emailData)
            };

            await _messageProducer.PublishMessage(emailEvent, SEND_EMAIL_QUEUE);
            _logger.LogInformation(
                "Published rejection email event for request {RequestId} to personal email {Email}",
                request.Id, requester.PersonalEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send rejection email for request {RequestId}",
                request.Id);
            // Don't throw - email sending failure shouldn't fail the rejection
        }
    }

    /// <summary>
    /// Formats request type code into a human-readable name
    /// </summary>
    private static string FormatRequestTypeName(string requestTypeCode)
    {
        return requestTypeCode switch
        {
            "PROFILE_ID_CHANGE" => "Profile ID Change",
            "PAID_LEAVE" => "Paid Leave",
            "UNPAID_LEAVE" => "Unpaid Leave",
            "PAID_SICK_LEAVE" => "Paid Sick Leave",
            "UNPAID_SICK_LEAVE" => "Unpaid Sick Leave",
            "WFH" => "Work From Home",
            _ => requestTypeCode.Replace("_", " ")
        };
    }

    private static int CalculateDuration(DateTime startDate, DateTime endDate)
    {
        // Calculate business days (excluding weekends)
        // For simplicity, we'll calculate total days including weekends
        // You can enhance this to exclude weekends if needed
        var days = (endDate.Date - startDate.Date).Days + 1;
        return Math.Max(1, days); // At least 1 day
    }
}

