using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Helpers;
using EmployeeApi.Services.RequestNotifications;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RequestEntity = EmployeeApi.Models.Request;

namespace EmployeeApi.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRequestNotificationService _notificationService;
    private readonly INotificationService _rabbitMqNotificationService;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly ILogger<RequestService> _logger;

    public RequestService(
        IRequestRepository requestRepository,
        IRequestTypeRepository requestTypeRepository,
        IEmployeeRepository employeeRepository,
        IRequestNotificationService notificationService,
        INotificationService rabbitMqNotificationService,
        ILeaveBalanceRepository leaveBalanceRepository,
        ILogger<RequestService> logger)
    {
        _requestRepository = requestRepository;
        _requestTypeRepository = requestTypeRepository;
        _employeeRepository = employeeRepository;
        _notificationService = notificationService;
        _rabbitMqNotificationService = rabbitMqNotificationService;
        _leaveBalanceRepository = leaveBalanceRepository;
        _logger = logger;
    }

    public async Task<PaginatedResponseDto<RequestDto>> GetRequestsAsync(
        long? employeeId = null,
        string? status = null,
        string? category = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20,
        long? managerId = null,
        bool filterByManagerReports = false,
        long? approverId = null,
        bool filterByApprover = false)
    {
        var requests = await _requestRepository.GetRequestsAsync(
            employeeId, status, category, dateFrom, dateTo, page, limit, managerId, filterByManagerReports, approverId, filterByApprover);

        var totalCount = await _requestRepository.GetRequestsCountAsync(
            employeeId, status, category, dateFrom, dateTo, managerId, filterByManagerReports, approverId, filterByApprover);

        var requestDtos = new List<RequestDto>();
        foreach (var request in requests)
        {
            requestDtos.Add(await MapToRequestDtoAsync(request));
        }

        return new PaginatedResponseDto<RequestDto>
        {
            Data = requestDtos,
            Pagination = new PaginationDto
            {
                Page = page,
                Limit = limit,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            }
        };
    }

    public async Task<RequestDetailsDto?> GetRequestByIdAsync(int id)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            return null;
        }

        return MapToRequestDetailsDto(request);
    }

    public async Task<RequestDto> CreateRequestAsync(CreateRequestDto dto, long requesterEmployeeId)
    {
        RequestTypeLookup? requestTypeLookup = null;

        // Support both requestTypeId (new) and RequestType (legacy) for backward compatibility
        if (dto.RequestTypeId.HasValue)
        {
            requestTypeLookup = await _requestTypeRepository.GetRequestTypeByIdAsync(dto.RequestTypeId.Value);
            if (requestTypeLookup == null)
            {
                throw new ArgumentException($"Invalid request type ID: {dto.RequestTypeId}");
            }
        }
        else if (!string.IsNullOrEmpty(dto.RequestType))
        {
            var normalizedType = dto.RequestType.ToUpper().Replace("-", "_");
            requestTypeLookup = await _requestTypeRepository.GetRequestTypeByCodeAsync(normalizedType);
            if (requestTypeLookup == null)
            {
                throw new ArgumentException($"Invalid request type: {dto.RequestType}");
            }
        }
        else
        {
            throw new ArgumentException("Either RequestTypeId or RequestType must be provided");
        }

        // Category-specific validation
        await ValidateRequestByCategoryAsync(requestTypeLookup, dto, requesterEmployeeId);

        // Build payload with attachments if provided
        string? payloadJson = null;
        if (dto.Payload.HasValue || (dto.Attachments != null && dto.Attachments.Count > 0))
        {
            var payloadDict = new Dictionary<string, object>();

            if (dto.Payload.HasValue)
            {
                // Merge existing payload
                var existingPayload = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.Payload.Value.GetRawText());
                if (existingPayload != null)
                {
                    foreach (var kvp in existingPayload)
                    {
                        payloadDict[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Add attachments if provided
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                payloadDict["attachments"] = dto.Attachments;
            }

            payloadJson = JsonSerializer.Serialize(payloadDict);
        }

        // Ensure DateTime values are UTC for PostgreSQL compatibility
        var effectiveFrom = dto.EffectiveFrom.HasValue
            ? DateTime.SpecifyKind(dto.EffectiveFrom.Value, DateTimeKind.Utc)
            : (DateTime?)null;
        var effectiveTo = dto.EffectiveTo.HasValue
            ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var request = new RequestEntity
        {
            RequestTypeId = requestTypeLookup.Id,
            RequesterEmployeeId = requesterEmployeeId,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Reason = dto.Reason,
            Payload = payloadJson,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRequest = await _requestRepository.CreateRequestAsync(request);

        // For PROFILE_ID_CHANGE requests, compute and store fieldChangeDetails in the payload
        if (requestTypeLookup.Code == "PROFILE_ID_CHANGE" && !string.IsNullOrEmpty(createdRequest.Payload))
        {
            var fieldChangeDetails = await ComputeFieldChangesAsync(createdRequest);
            if (fieldChangeDetails != null && fieldChangeDetails.Count > 0)
            {
                var payloadDict = JsonSerializer.Deserialize<Dictionary<string, object>>(createdRequest.Payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payloadDict != null)
                {
                    // Store fieldChangeDetails in the payload
                    payloadDict["fieldChangeDetails"] = fieldChangeDetails;
                    createdRequest.Payload = JsonSerializer.Serialize(payloadDict);
                    createdRequest = await _requestRepository.UpdateRequestAsync(createdRequest);
                }
            }
        }

        return await MapToRequestDtoAsync(createdRequest);
    }

    public async Task<RequestDto> UpdateRequestAsync(int id, UpdateRequestDto dto, long requesterEmployeeId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.RequesterEmployeeId != requesterEmployeeId)
        {
            throw new UnauthorizedAccessException("You can only update your own requests");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be updated");
        }

        if (dto.EffectiveFrom.HasValue)
        {
            request.EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom.Value, DateTimeKind.Utc);
        }

        if (dto.EffectiveTo.HasValue)
        {
            request.EffectiveTo = DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc);
        }

        if (!string.IsNullOrEmpty(dto.Reason))
        {
            request.Reason = dto.Reason;
        }

        if (dto.Payload.HasValue)
        {
            request.Payload = JsonSerializer.Serialize(dto.Payload.Value);
        }

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);
        return await MapToRequestDtoAsync(updatedRequest);
    }

    public async Task<bool> CancelRequestAsync(int id, long requesterEmployeeId, string? comment = null)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        // Verify request type is PROFILE_ID_CHANGE (category = 'profile')
        var requestTypeCode = request.RequestTypeLookup?.Code?.ToUpper() ?? "";
        var isProfileIdChangeRequest = requestTypeCode == "PROFILE_ID_CHANGE";

        if (isProfileIdChangeRequest && request.RequestTypeLookup?.Category?.ToLower() != "profile")
        {
            throw new InvalidOperationException("Request type mismatch");
        }

        if (request.RequesterEmployeeId != requesterEmployeeId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own requests");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be cancelled");
        }

        request.Status = RequestStatus.Cancelled;

        // Store cancellation comment in payload if provided
        if (!string.IsNullOrWhiteSpace(comment))
        {
            var payloadDict = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(request.Payload))
            {
                try
                {
                    var existingPayload = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Payload);
                    if (existingPayload != null)
                    {
                        foreach (var kvp in existingPayload)
                        {
                            payloadDict[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch
                {
                    // If parsing fails, start with empty dict
                }
            }

            payloadDict["cancellationComment"] = comment;
            request.Payload = JsonSerializer.Serialize(payloadDict);
        }

        await _requestRepository.UpdateRequestAsync(request);
        return true;
    }

    public async Task<RequestDto> ApproveRequestAsync(int id, long approverEmployeeId, string? comment)
    {
        // Common validation
        var request = await ValidateRequestForApprovalAsync(id, approverEmployeeId);
        var requestTypeCode = request.RequestTypeLookup?.Code?.ToUpper() ?? "";

        // Category-specific approval logic (e.g., apply changes for profile requests)
        await OnRequestApprovedAsync(request, requestTypeCode);

        // Common approval updates
        request.Status = RequestStatus.Approved;
        request.ApproverEmployeeId = approverEmployeeId;
        request.ApprovalComment = comment;

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);

        // Send approval email for profile and time-off requests (not timesheet)
        await _notificationService.SendApprovalEmailAsync(updatedRequest, comment);

        // Send notification to employee when time-off request is approved
        var category = updatedRequest.RequestTypeLookup?.Category?.ToLower() ?? "";
        if (category == "time-off")
        {
            try
            {
                var requester = updatedRequest.Requester ?? await _employeeRepository.GetByIdAsync(updatedRequest.RequesterEmployeeId);
                if (requester != null)
                {
                    var requestTypeName = FormatRequestTypeName(requestTypeCode);
                    var approver = updatedRequest.Approver ?? await _employeeRepository.GetByIdAsync(updatedRequest.ApproverEmployeeId ?? 0);
                    var approverName = approver?.FullName ?? "Manager";

                    // Build message with date range if available
                    string message;
                    if (updatedRequest.EffectiveFrom.HasValue && updatedRequest.EffectiveTo.HasValue)
                    {
                        var startDate = updatedRequest.EffectiveFrom.Value.ToString("MMMM dd, yyyy");
                        var endDate = updatedRequest.EffectiveTo.Value.ToString("MMMM dd, yyyy");
                        var duration = CalculateDuration(updatedRequest.EffectiveFrom.Value, updatedRequest.EffectiveTo.Value);
                        message = $"Your {requestTypeName.ToLower()} request from {startDate} to {endDate} ({duration} day{(duration != 1 ? "s" : "")}) has been approved by {approverName}.";
                    }
                    else
                    {
                        message = $"Your {requestTypeName.ToLower()} request has been approved by {approverName}.";
                    }

                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        message += $" Comment: {comment}";
                    }

                    await _rabbitMqNotificationService.SendNotificationAsync(new NotificationEvent
                    {
                        EmpId = requester.Id,
                        Title = $"{requestTypeName} Request Approved",
                        Message = message,
                        Type = "success"
                    });

                    _logger.LogInformation(
                        "Sent approval notification to employee {EmployeeId} for time-off request {RequestId}",
                        requester.Id, updatedRequest.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send approval notification to employee for time-off request {RequestId}",
                    updatedRequest.Id);
                // Don't throw - notification failure shouldn't fail the approval
            }
        }

        return await MapToRequestDtoAsync(updatedRequest);
    }

    public async Task<RequestDto> RejectRequestAsync(int id, long approverEmployeeId, string reason)
    {
        // Common validation
        var request = await ValidateRequestForApprovalAsync(id, approverEmployeeId);
        var requestTypeCode = request.RequestTypeLookup?.Code?.ToUpper() ?? "";

        // Category-specific rejection logic (if needed in the future)
        await OnRequestRejectedAsync(request, requestTypeCode, reason);

        // Common rejection updates
        request.Status = RequestStatus.Rejected;
        request.ApproverEmployeeId = approverEmployeeId;
        request.RejectionReason = reason;

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);

        // Send rejection email for profile and time-off requests (not timesheet)
        await _notificationService.SendRejectionEmailAsync(updatedRequest, reason);

        return await MapToRequestDtoAsync(updatedRequest);
    }

    public async Task<RequestsSummaryDto> GetRequestsSummaryAsync(
        long? employeeId = null,
        string? month = null,
        string? requestType = null)
    {
        var byStatus = await _requestRepository.GetRequestsSummaryByStatusAsync(employeeId, month, requestType);
        var byType = await _requestRepository.GetRequestsSummaryByTypeAsync(employeeId, month, requestType);

        var total = byStatus.Values.Sum();

        return new RequestsSummaryDto
        {
            Total = total,
            ByStatus = new StatusCountDto
            {
                Pending = byStatus.GetValueOrDefault("pending", 0),
                Approved = byStatus.GetValueOrDefault("approved", 0),
                Rejected = byStatus.GetValueOrDefault("rejected", 0),
                Cancelled = byStatus.GetValueOrDefault("cancelled", 0)
            },
            ByType = byType
        };
    }

    private async Task<RequestDto> MapToRequestDtoAsync(RequestEntity request)
    {
        var requestTypeCode = request.RequestTypeLookup?.Code ?? "UNKNOWN";
        var isTimeOffRequest = IsTimeOffRequestType(requestTypeCode);
        var isProfileIdChangeRequest = requestTypeCode == "PROFILE_ID_CHANGE";

        // Extract attachments from payload
        var attachments = ExtractAttachments(request.Payload);

        // Calculate duration for time-off requests
        int? duration = null;
        if (isTimeOffRequest && request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue)
        {
            duration = CalculateDuration(request.EffectiveFrom.Value, request.EffectiveTo.Value);
        }

        // Format dates
        string? startDate = null;
        string? endDate = null;
        if (isTimeOffRequest && request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue)
        {
            startDate = request.EffectiveFrom.Value.ToString("yyyy-MM-dd");
            endDate = request.EffectiveTo.Value.ToString("yyyy-MM-dd");
        }

        // Get employee information
        var employee = request.Requester;

        var dto = new RequestDto
        {
            Id = request.Id.ToString(),
            Type = requestTypeCode,
            EmployeeId = request.RequesterEmployeeId.ToString(),
            EmployeeName = employee?.FullName,
            EmployeeEmail = employee?.Email,
            EmployeeAvatar = null, // TODO: Add avatar URL if available
            Department = employee?.Department?.Name,
            Status = request.Status.ToApiString(),
            StartDate = startDate,
            EndDate = endDate,
            Duration = duration,
            SubmittedDate = request.RequestedAt.ToString("O"), // ISO 8601 format
            Reason = request.Reason,
            Attachments = attachments
        };

        // Add payload, fieldChanges, and fieldChangeDetails for profile ID change requests
        if (isProfileIdChangeRequest)
        {
            if (!string.IsNullOrEmpty(request.Payload))
            {
                try
                {
                    dto.Payload = JsonSerializer.Deserialize<JsonElement>(request.Payload);
                }
                catch
                {
                    // If payload parsing fails, leave it null
                }
            }

            // Try to get fieldChangeDetails from stored payload, otherwise compute it
            List<FieldChangeDetailDto>? fieldChanges = null;
            if (!string.IsNullOrEmpty(request.Payload))
            {
                try
                {
                    var payloadDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.Payload, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (payloadDict != null && payloadDict.TryGetValue("fieldChangeDetails", out var fieldChangeDetailsElement))
                    {
                        fieldChanges = JsonSerializer.Deserialize<List<FieldChangeDetailDto>>(fieldChangeDetailsElement.GetRawText(), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                catch
                {
                    // If parsing fails, fall through to compute
                }
            }

            // If not stored, compute field changes by comparing payload with current employee data
            if (fieldChanges == null)
            {
                fieldChanges = await ComputeFieldChangesAsync(request);
            }

            dto.FieldChanges = fieldChanges.Select(fc => fc.FieldLabel).ToList();
            dto.FieldChangeDetails = fieldChanges;
        }

        // Add approval/rejection information
        if (request.Approver != null)
        {
            dto.ApproverName = request.Approver.FullName;
        }
        dto.ApprovalComment = request.ApprovalComment;
        dto.RejectionReason = request.RejectionReason;

        return dto;
    }

    private bool IsApprovalRequestType(string requestTypeCode)
    {
        var normalized = requestTypeCode.ToUpper();
        return normalized == "TIMESHEET_WEEKLY" ||
               normalized == "PAID_LEAVE" ||
               normalized == "UNPAID_LEAVE" ||
               normalized == "PAID_SICK_LEAVE" ||
               normalized == "UNPAID_SICK_LEAVE" ||
               normalized == "WFH";
    }

    private bool IsTimeOffRequestType(string requestTypeCode)
    {
        var normalized = requestTypeCode.ToUpper();
        return normalized == "PAID_LEAVE" ||
               normalized == "UNPAID_LEAVE" ||
               normalized == "PAID_SICK_LEAVE" ||
               normalized == "UNPAID_SICK_LEAVE" ||
               normalized == "WFH";
    }

    private List<string> ExtractAttachments(string? payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return new List<string>();
        }

        try
        {
            var jsonDoc = JsonDocument.Parse(payload);
            if (jsonDoc.RootElement.TryGetProperty("attachments", out var attachmentsElement))
            {
                if (attachmentsElement.ValueKind == JsonValueKind.Array)
                {
                    return attachmentsElement.EnumerateArray()
                        .Select(a => a.GetString() ?? "")
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }
        }
        catch
        {
            // If payload parsing fails, return empty list
        }

        return new List<string>();
    }

    private int CalculateDuration(DateTime startDate, DateTime endDate)
    {
        // Calculate business days (excluding weekends)
        // For simplicity, we'll calculate total days including weekends
        // You can enhance this to exclude weekends if needed
        var days = (endDate.Date - startDate.Date).Days + 1;
        return Math.Max(1, days); // At least 1 day
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

    private RequestDetailsDto MapToRequestDetailsDto(RequestEntity request)
    {
        return new RequestDetailsDto
        {
            Id = request.Id,
            RequestType = request.RequestTypeLookup?.Code ?? "UNKNOWN",
            RequesterEmployeeId = request.RequesterEmployeeId,
            ApproverEmployeeId = request.ApproverEmployeeId,
            Status = request.Status.ToApiString(),
            RequestedAt = request.RequestedAt,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Reason = request.Reason,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            Requester = request.Requester != null ? new EmployeeSummaryDto
            {
                Id = request.Requester.Id,
                Name = request.Requester.FullName,
                Email = request.Requester.Email,
                Department = request.Requester.Department?.Name
            } : null,
            Approver = request.Approver != null ? new EmployeeSummaryDto
            {
                Id = request.Approver.Id,
                Name = request.Approver.FullName,
                Email = request.Approver.Email,
                Department = request.Approver.Department?.Name
            } : null,
            Payload = !string.IsNullOrEmpty(request.Payload)
                ? JsonSerializer.Deserialize<JsonElement>(request.Payload)
                : null,
            ApprovalComment = request.ApprovalComment,
            RejectionReason = request.RejectionReason
        };
    }

    // ========== Common Validation Methods ==========

    /// <summary>
    /// Common validation for request approval/rejection
    /// </summary>
    private async Task<RequestEntity> ValidateRequestForApprovalAsync(int id, long approverEmployeeId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be approved or rejected");
        }

        // Verify manager relationship for approval request types
        var requestTypeCode = request.RequestTypeLookup?.Code?.ToUpper() ?? "";
        var isApprovalRequest = IsApprovalRequestType(requestTypeCode);

        if (isApprovalRequest)
        {
            var isEmployeeUnderManager = await _requestRepository.IsEmployeeUnderManagerAsync(
                request.RequesterEmployeeId, approverEmployeeId);

            if (!isEmployeeUnderManager)
            {
                throw new UnauthorizedAccessException("Cannot approve/reject request from employee outside your team");
            }
        }

        return request;
    }

    // ========== Category-Specific Handlers ==========

    /// <summary>
    /// Validates request based on category/type during creation
    /// </summary>
    private async Task ValidateRequestByCategoryAsync(RequestTypeLookup requestType, CreateRequestDto dto, long requesterEmployeeId)
    {
        var category = requestType.Category?.ToLower() ?? "";
        var code = requestType.Code?.ToUpper() ?? "";

        switch (category)
        {
            case "profile":
                if (code == "PROFILE_ID_CHANGE")
                {
                    await ValidateProfileIdChangeRequestAsync(dto, requesterEmployeeId);
                }
                // Add other profile request validations here
                break;

            case "time-off":
                // Time-off requests are validated in TimeOffService
                // This is a generic endpoint, so minimal validation here
                break;

            case "timesheet":
                // Timesheet requests are validated in TimesheetService
                // This is a generic endpoint, so minimal validation here
                break;

            default:
                // No category-specific validation needed
                break;
        }
    }

    /// <summary>
    /// Handles category-specific logic when a request is approved
    /// </summary>
    private async Task OnRequestApprovedAsync(RequestEntity request, string requestTypeCode)
    {
        var category = request.RequestTypeLookup?.Category?.ToLower() ?? "";

        switch (category)
        {
            case "profile":
                if (requestTypeCode == "PROFILE_ID_CHANGE")
                {
                    await ApplyProfileIdChangesAsync(request);
                }
                // Add other profile request approval logic here
                break;

            case "time-off":
                // Deduct leave balance for paid leave types
                await DeductLeaveBalanceAsync(request, requestTypeCode);
                break;

            case "timesheet":
                // Timesheet specific approval logic
                // Currently handled elsewhere, but can be added here if needed
                break;

            default:
                // No category-specific approval logic needed
                break;
        }
    }

    /// <summary>
    /// Handles category-specific logic when a request is rejected
    /// </summary>
    private async Task OnRequestRejectedAsync(RequestEntity request, string requestTypeCode, string reason)
    {
        var category = request.RequestTypeLookup?.Category?.ToLower() ?? "";

        switch (category)
        {
            case "profile":
                // Profile-specific rejection logic (e.g., cleanup, notifications)
                // Currently no special handling needed
                break;

            case "time-off":
                // Time-off specific rejection logic
                // Currently no special handling needed
                break;

            case "timesheet":
                // Timesheet specific rejection logic
                // Currently no special handling needed
                break;

            default:
                // No category-specific rejection logic needed
                break;
        }

        // Return Task.CompletedTask to satisfy async signature
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deducts leave balance when a paid time-off request is approved
    /// </summary>
    private async Task DeductLeaveBalanceAsync(RequestEntity request, string requestTypeCode)
    {
        // Only deduct for paid leave types
        if (requestTypeCode != "PAID_LEAVE" && requestTypeCode != "PAID_SICK_LEAVE")
        {
            return;
        }

        // Ensure we have valid dates
        if (!request.EffectiveFrom.HasValue || !request.EffectiveTo.HasValue)
        {
            _logger.LogWarning(
                "Cannot deduct leave balance for request {RequestId}: missing EffectiveFrom or EffectiveTo dates",
                request.Id);
            return;
        }

        try
        {
            // Map request type to balance type
            var balanceType = requestTypeCode == "PAID_LEAVE" ? "Annual Leave" : "Sick Leave";
            var year = request.EffectiveFrom.Value.Year;

            // Calculate duration
            var duration = CalculateDuration(request.EffectiveFrom.Value, request.EffectiveTo.Value);

            // Get or create leave balance
            var balance = await _leaveBalanceRepository.GetLeaveBalanceAsync(
                request.RequesterEmployeeId,
                balanceType,
                year);

            if (balance == null)
            {
                _logger.LogWarning(
                    "Leave balance not found for employee {EmployeeId}, balance type {BalanceType}, year {Year}. Creating new balance.",
                    request.RequesterEmployeeId, balanceType, year);

                // Create a new balance with default total
                var defaultTotals = new Dictionary<string, decimal>
                {
                    { "Annual Leave", 15 },
                    { "Sick Leave", 10 },
                    { "Parental Leave", 14 },
                    { "Other Leave", 5 }
                };

                balance = new LeaveBalance
                {
                    EmployeeId = request.RequesterEmployeeId,
                    BalanceType = balanceType,
                    Year = year,
                    Total = defaultTotals.GetValueOrDefault(balanceType, 0),
                    Used = 0
                };
            }

            // Update used days
            balance.Used += duration;

            // Ensure used doesn't exceed total (shouldn't happen if validation worked, but safety check)
            if (balance.Used > balance.Total)
            {
                _logger.LogWarning(
                    "Leave balance used ({Used}) exceeds total ({Total}) for employee {EmployeeId}, balance type {BalanceType}, year {Year}",
                    balance.Used, balance.Total, request.RequesterEmployeeId, balanceType, year);
            }

            // Save updated balance
            await _leaveBalanceRepository.CreateOrUpdateLeaveBalanceAsync(balance);

            _logger.LogInformation(
                "Deducted {Duration} days from {BalanceType} balance for employee {EmployeeId} (year {Year}). New used: {Used}/{Total}",
                duration, balanceType, request.RequesterEmployeeId, year, balance.Used, balance.Total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to deduct leave balance for request {RequestId}",
                request.Id);
            // Don't throw - balance update failure shouldn't fail the approval
            // But log it so it can be investigated
        }
    }

    /// <summary>
    /// Validates PROFILE_ID_CHANGE requests
    /// </summary>
    private async Task ValidateProfileIdChangeRequestAsync(CreateRequestDto dto, long requesterEmployeeId)
    {
        var validationErrors = new List<(string Field, string Message)>();

        // Check if there's already a pending PROFILE_ID_CHANGE request for this employee
        var existingPendingRequests = await _requestRepository.GetRequestsAsync(
            employeeId: requesterEmployeeId,
            status: RequestStatus.Pending.ToString(),
            category: "profile",
            page: 1,
            limit: 1
        );

        // Filter to only PROFILE_ID_CHANGE requests
        var hasPendingProfileIdChange = existingPendingRequests
            .Any(r => r.RequestTypeLookup?.Code == "PROFILE_ID_CHANGE");

        if (hasPendingProfileIdChange)
        {
            validationErrors.Add(("request", "You already have a pending profile change request. Please wait for it to be approved, rejected, or cancelled before creating a new one."));
            throw new ArgumentException("Validation failed: " + string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}")));
        }

        // Validate reason
        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            validationErrors.Add(("reason", "Reason is required"));
        }
        else if (dto.Reason.Length > 500)
        {
            validationErrors.Add(("reason", "Reason must not exceed 500 characters"));
        }

        // Parse and validate payload
        if (!dto.Payload.HasValue)
        {
            validationErrors.Add(("payload", "Payload is required for profile ID change requests"));
            throw new ArgumentException("Validation failed: " + string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}")));
        }

        ProfileIdChangePayloadDto? payload = null;
        try
        {
            payload = JsonSerializer.Deserialize<ProfileIdChangePayloadDto>(dto.Payload.Value.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            validationErrors.Add(("payload", $"Invalid payload format: {ex.Message}"));
            throw new ArgumentException("Validation failed: " + string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}")));
        }

        if (payload == null)
        {
            validationErrors.Add(("payload", "Payload cannot be null"));
            throw new ArgumentException("Validation failed: " + string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}")));
        }

        // Validate that at least one field is provided
        bool hasAnyField = !string.IsNullOrWhiteSpace(payload.FullName) ||
                          !string.IsNullOrWhiteSpace(payload.FirstName) ||
                          !string.IsNullOrWhiteSpace(payload.LastName) ||
                          !string.IsNullOrWhiteSpace(payload.Nationality) ||
                          !string.IsNullOrWhiteSpace(payload.SocialInsuranceNumber) ||
                          !string.IsNullOrWhiteSpace(payload.TaxId) ||
                          payload.NationalId != null;

        if (!hasAnyField)
        {
            validationErrors.Add(("payload", "At least one field (fullName, firstName, lastName, nationality, nationalId, socialInsuranceNumber, or taxId) must be provided"));
        }

        // Validate fullName if provided
        if (!string.IsNullOrWhiteSpace(payload.FullName))
        {
            if (payload.FullName.Length < 2 || payload.FullName.Length > 200)
            {
                validationErrors.Add(("payload.fullName", "Full name must be between 2 and 200 characters"));
            }
        }

        // Validate firstName/lastName if provided
        if (!string.IsNullOrWhiteSpace(payload.FirstName))
        {
            if (payload.FirstName.Length < 2 || payload.FirstName.Length > 100)
            {
                validationErrors.Add(("payload.firstName", "First name must be between 2 and 100 characters"));
            }
        }

        if (!string.IsNullOrWhiteSpace(payload.LastName))
        {
            if (payload.LastName.Length < 2 || payload.LastName.Length > 100)
            {
                validationErrors.Add(("payload.lastName", "Last name must be between 2 and 100 characters"));
            }
        }

        // Validate nationalId if provided
        if (payload.NationalId != null)
        {
            bool hasNationalIdField = !string.IsNullOrWhiteSpace(payload.NationalId.Number) ||
                                     !string.IsNullOrWhiteSpace(payload.NationalId.IssuedDate) ||
                                     !string.IsNullOrWhiteSpace(payload.NationalId.ExpirationDate) ||
                                     !string.IsNullOrWhiteSpace(payload.NationalId.IssuedBy);

            if (!hasNationalIdField)
            {
                validationErrors.Add(("payload.nationalId", "At least one field within nationalId must be provided"));
            }

            // Validate nationalId.number
            if (!string.IsNullOrWhiteSpace(payload.NationalId.Number))
            {
                if (payload.NationalId.Number.Length > 50)
                {
                    validationErrors.Add(("payload.nationalId.number", "National ID number must not exceed 50 characters"));
                }
            }

            // Validate dates
            DateOnly? issuedDate = null;
            DateOnly? expirationDate = null;

            if (!string.IsNullOrWhiteSpace(payload.NationalId.IssuedDate))
            {
                if (!DateOnly.TryParse(payload.NationalId.IssuedDate, out var parsedIssuedDate))
                {
                    validationErrors.Add(("payload.nationalId.issuedDate", "Issued date must be in yyyy-MM-dd format"));
                }
                else
                {
                    issuedDate = parsedIssuedDate;
                    // Validate that issued date is not in the future
                    if (issuedDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
                    {
                        validationErrors.Add(("payload.nationalId.issuedDate", "Issued date cannot be in the future"));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(payload.NationalId.ExpirationDate))
            {
                if (!DateOnly.TryParse(payload.NationalId.ExpirationDate, out var parsedExpirationDate))
                {
                    validationErrors.Add(("payload.nationalId.expirationDate", "Expiration date must be in yyyy-MM-dd format"));
                }
                else
                {
                    expirationDate = parsedExpirationDate;
                }
            }

            // Validate expiration date is after issued date
            if (issuedDate.HasValue && expirationDate.HasValue)
            {
                if (expirationDate.Value <= issuedDate.Value)
                {
                    validationErrors.Add(("payload.nationalId.expirationDate", "Expiration date must be after issued date"));
                }
            }
        }

        // Validate socialInsuranceNumber
        if (!string.IsNullOrWhiteSpace(payload.SocialInsuranceNumber))
        {
            if (payload.SocialInsuranceNumber.Length > 50)
            {
                validationErrors.Add(("payload.socialInsuranceNumber", "Social insurance number must not exceed 50 characters"));
            }
        }

        // Validate taxId
        if (!string.IsNullOrWhiteSpace(payload.TaxId))
        {
            if (payload.TaxId.Length > 50)
            {
                validationErrors.Add(("payload.taxId", "Tax ID must not exceed 50 characters"));
            }
        }

        // Validate comment
        if (!string.IsNullOrWhiteSpace(payload.Comment))
        {
            if (payload.Comment.Length > 1000)
            {
                validationErrors.Add(("payload.comment", "Comment must not exceed 1000 characters"));
            }
        }

        // Validate attachments
        var attachments = dto.Attachments ?? payload.Attachments;
        if (attachments != null)
        {
            if (attachments.Count > 5)
            {
                validationErrors.Add(("attachments", "Maximum 5 attachments allowed"));
            }

            foreach (var attachment in attachments)
            {
                if (string.IsNullOrWhiteSpace(attachment) || !Uri.TryCreate(attachment, UriKind.Absolute, out _))
                {
                    validationErrors.Add(("attachments", $"Invalid attachment URL: {attachment}"));
                }
            }
        }

        if (validationErrors.Count > 0)
        {
            throw new ArgumentException("Validation failed: " + string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.Message}")));
        }
    }

    private async Task<List<FieldChangeDetailDto>> ComputeFieldChangesAsync(RequestEntity request)
    {
        var fieldChanges = new List<FieldChangeDetailDto>();

        if (string.IsNullOrEmpty(request.Payload))
        {
            return fieldChanges;
        }

        // Get current employee data
        // Since we prevent multiple pending requests, we can always get the current employee data
        // and it will be accurate (no other pending request can modify it)
        var employee = await _employeeRepository.GetByIdAsync(request.RequesterEmployeeId);
        if (employee == null)
        {
            return fieldChanges;
        }

        try
        {
            // Parse payload, excluding fieldChangeDetails if present
            var payloadDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.Payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payloadDict == null)
            {
                return fieldChanges;
            }

            // Extract the actual payload (new values), excluding fieldChangeDetails
            var payloadWithoutMetadata = new Dictionary<string, object>();
            foreach (var kvp in payloadDict)
            {
                var keyLower = kvp.Key.ToLower();
                if (keyLower != "fieldchangedetails" && keyLower != "oldvalues")
                {
                    payloadWithoutMetadata[kvp.Key] = kvp.Value;
                }
            }

            var payloadJson = JsonSerializer.Serialize(payloadWithoutMetadata);
            var payload = JsonSerializer.Deserialize<ProfileIdChangePayloadDto>(payloadJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                return fieldChanges;
            }

            // Check fullName change (independent field)
            if (!string.IsNullOrWhiteSpace(payload.FullName) && payload.FullName != employee.FullName)
            {
                fieldChanges.Add(new FieldChangeDetailDto
                {
                    FieldLabel = "Legal Full Name",
                    OldValue = employee.FullName,
                    NewValue = payload.FullName
                });
            }

            // Check firstName change (independent field)
            if (!string.IsNullOrWhiteSpace(payload.FirstName) && payload.FirstName != employee.FirstName)
            {
                fieldChanges.Add(new FieldChangeDetailDto
                {
                    FieldLabel = "First Name",
                    OldValue = employee.FirstName,
                    NewValue = payload.FirstName
                });
            }

            // Check lastName change (independent field)
            if (!string.IsNullOrWhiteSpace(payload.LastName) && payload.LastName != employee.LastName)
            {
                fieldChanges.Add(new FieldChangeDetailDto
                {
                    FieldLabel = "Last Name",
                    OldValue = employee.LastName,
                    NewValue = payload.LastName
                });
            }

            // Check nationality change
            if (!string.IsNullOrWhiteSpace(payload.Nationality) && payload.Nationality != employee.NationalIdCountry)
            {
                fieldChanges.Add(new FieldChangeDetailDto
                {
                    FieldLabel = "Nationality",
                    OldValue = employee.NationalIdCountry,
                    NewValue = payload.Nationality
                });
            }

            // Check nationalId changes
            if (payload.NationalId != null)
            {
                if (!string.IsNullOrWhiteSpace(payload.NationalId.Number) && payload.NationalId.Number != employee.NationalIdNumber)
                {
                    fieldChanges.Add(new FieldChangeDetailDto
                    {
                        FieldLabel = "National ID Number",
                        OldValue = employee.NationalIdNumber,
                        NewValue = payload.NationalId.Number
                    });
                }

                if (!string.IsNullOrWhiteSpace(payload.NationalId.IssuedDate))
                {
                    if (DateOnly.TryParse(payload.NationalId.IssuedDate, out var newIssuedDate))
                    {
                        var oldValue = employee.NationalIdIssuedDate?.ToString("yyyy-MM-dd");
                        var newValue = newIssuedDate.ToString("yyyy-MM-dd");

                        if (oldValue != newValue)
                        {
                            fieldChanges.Add(new FieldChangeDetailDto
                            {
                                FieldLabel = "National ID Issued Date",
                                OldValue = oldValue,
                                NewValue = newValue
                            });
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(payload.NationalId.ExpirationDate))
                {
                    if (DateOnly.TryParse(payload.NationalId.ExpirationDate, out var newExpirationDate))
                    {
                        var oldValue = employee.NationalIdExpirationDate?.ToString("yyyy-MM-dd");
                        var newValue = newExpirationDate.ToString("yyyy-MM-dd");

                        if (oldValue != newValue)
                        {
                            fieldChanges.Add(new FieldChangeDetailDto
                            {
                                FieldLabel = "National ID Expiration Date",
                                OldValue = oldValue,
                                NewValue = newValue
                            });
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(payload.NationalId.IssuedBy) && payload.NationalId.IssuedBy != employee.NationalIdIssuedBy)
                {
                    fieldChanges.Add(new FieldChangeDetailDto
                    {
                        FieldLabel = "National ID Issued By",
                        OldValue = employee.NationalIdIssuedBy,
                        NewValue = payload.NationalId.IssuedBy
                    });
                }
            }

            // Check socialInsuranceNumber change
            if (!string.IsNullOrWhiteSpace(payload.SocialInsuranceNumber) && payload.SocialInsuranceNumber != employee.SocialInsuranceNumber)
            {
                fieldChanges.Add(new FieldChangeDetailDto
                {
                    FieldLabel = "Social Insurance Number",
                    OldValue = employee.SocialInsuranceNumber,
                    NewValue = payload.SocialInsuranceNumber
                });
            }

            // Check taxId change
            if (!string.IsNullOrWhiteSpace(payload.TaxId) && payload.TaxId != employee.TaxId)
            {
                fieldChanges.Add(new FieldChangeDetailDto
                {
                    FieldLabel = "Tax ID",
                    OldValue = employee.TaxId,
                    NewValue = payload.TaxId
                });
            }
        }
        catch
        {
            // If payload parsing fails, return empty list
        }

        return fieldChanges;
    }

    private async Task ApplyProfileIdChangesAsync(RequestEntity request)
    {
        if (string.IsNullOrEmpty(request.Payload))
        {
            return;
        }

        // Use the tracked employee from the request if available, otherwise fetch it
        // The request.Requester is already tracked since GetRequestByIdAsync includes it
        var employee = request.Requester;
        var needsUpdate = false; // Track if we need to call Update (only if we fetched separately)

        if (employee == null)
        {
            // Fallback: get employee if Requester wasn't loaded (shouldn't happen, but safety check)
            employee = await _employeeRepository.GetByIdAsync(request.RequesterEmployeeId);
            if (employee == null)
            {
                throw new Exception($"Employee with ID {request.RequesterEmployeeId} not found");
            }
            // If we had to fetch it, we need to attach it for tracking
            needsUpdate = true;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<ProfileIdChangePayloadDto>(request.Payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null)
            {
                return;
            }

            // Apply fullName change
            if (!string.IsNullOrWhiteSpace(payload.FullName))
            {
                employee.FullName = payload.FullName.Trim();
            }

            // Apply firstName change
            if (!string.IsNullOrWhiteSpace(payload.FirstName))
            {
                employee.FirstName = payload.FirstName.Trim();
            }

            // Apply lastName change
            if (!string.IsNullOrWhiteSpace(payload.LastName))
            {
                employee.LastName = payload.LastName.Trim();
            }

            // Apply nationality change (maps to NationalIdCountry)
            if (!string.IsNullOrWhiteSpace(payload.Nationality))
            {
                employee.NationalIdCountry = payload.Nationality.Trim();
            }

            // Apply nationalId changes
            if (payload.NationalId != null)
            {
                if (!string.IsNullOrWhiteSpace(payload.NationalId.Number))
                {
                    employee.NationalIdNumber = payload.NationalId.Number.Trim();
                }
                if (!string.IsNullOrWhiteSpace(payload.NationalId.IssuedDate))
                {
                    if (DateOnly.TryParse(payload.NationalId.IssuedDate, out var issuedDate))
                    {
                        employee.NationalIdIssuedDate = issuedDate;
                    }
                }
                if (!string.IsNullOrWhiteSpace(payload.NationalId.ExpirationDate))
                {
                    if (DateOnly.TryParse(payload.NationalId.ExpirationDate, out var expirationDate))
                    {
                        employee.NationalIdExpirationDate = expirationDate;
                    }
                }
                if (!string.IsNullOrWhiteSpace(payload.NationalId.IssuedBy))
                {
                    employee.NationalIdIssuedBy = payload.NationalId.IssuedBy.Trim();
                }
            }

            // Apply socialInsuranceNumber change
            if (!string.IsNullOrWhiteSpace(payload.SocialInsuranceNumber))
            {
                employee.SocialInsuranceNumber = payload.SocialInsuranceNumber.Trim();
            }

            // Apply taxId change
            if (!string.IsNullOrWhiteSpace(payload.TaxId))
            {
                employee.TaxId = payload.TaxId.Trim();
            }

            // Update the updated timestamp (use DateTime.Now for timestamp without time zone)
            employee.UpdatedAt = DateTime.Now;

            // Save changes
            // Only call Update if we fetched the employee separately (not from request.Requester)
            // If employee is from request.Requester, it's already tracked, so just save
            if (needsUpdate)
            {
                _employeeRepository.Update(employee);
            }
            await _employeeRepository.SaveChangesAsync();
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse profile change payload: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to apply profile changes: {ex.Message}", ex);
        }
    }

}
