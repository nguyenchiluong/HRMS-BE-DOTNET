using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Helpers;
using System.Text.Json;

namespace EmployeeApi.Services;

public class TimeOffService : ITimeOffService
{
    private readonly IRequestRepository _requestRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly ILogger<TimeOffService> _logger;

    public TimeOffService(
        IRequestRepository requestRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IRequestTypeRepository requestTypeRepository,
        ILogger<TimeOffService> logger)
    {
        _requestRepository = requestRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _requestTypeRepository = requestTypeRepository;
        _logger = logger;
    }

    public async Task<TimeOffRequestResponseDto> SubmitTimeOffRequestAsync(
        SubmitTimeOffRequestDto dto,
        long employeeId,
        List<string>? attachmentUrls = null)
    {
        // Validate dates
        if (dto.StartDate > dto.EndDate)
        {
            throw new ArgumentException("Start date must be before or equal to end date");
        }

        if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Start date cannot be in the past");
        }

        // Calculate duration
        var duration = await CalculateDurationAsync(dto.StartDate, dto.EndDate);

        // Look up request type from database (expects uppercase snake_case)
        var normalizedType = dto.Type.ToUpper().Replace("-", "_");
        var requestTypeLookup = await _requestTypeRepository.GetRequestTypeByCodeAsync(normalizedType);
        if (requestTypeLookup == null)
        {
            throw new ArgumentException($"Invalid request type: {dto.Type}");
        }

        // Validate sick leave attachments if duration > 3 days
        if ((requestTypeLookup.Code == "PAID_SICK_LEAVE" || requestTypeLookup.Code == "UNPAID_SICK_LEAVE")
            && duration > 3)
        {
            if (attachmentUrls == null || attachmentUrls.Count == 0)
            {
                throw new ArgumentException("Medical certificate attachment is required for sick leave requests longer than 3 days");
            }
        }

        // Check leave balance for paid leave types
        if (requestTypeLookup.Code == "PAID_LEAVE" || requestTypeLookup.Code == "PAID_SICK_LEAVE")
        {
            var balanceType = requestTypeLookup.Code == "PAID_LEAVE" ? "Annual Leave" : "Sick Leave";
            var year = dto.StartDate.Year;
            var balance = await _leaveBalanceRepository.GetLeaveBalanceAsync(employeeId, balanceType, year);

            if (balance == null)
            {
                throw new InvalidOperationException($"No leave balance found for {balanceType}");
            }

            var used = await GetUsedLeaveDaysAsync(employeeId, balanceType, year);
            var remaining = balance.Total - used;

            if (remaining < duration)
            {
                throw new InvalidOperationException($"Insufficient leave balance. Remaining: {remaining} days, Requested: {duration} days");
            }
        }

        // Generate request ID
        var requestId = await GenerateRequestIdAsync();

        // Create payload with attachments and request ID
        var payload = new Dictionary<string, object>
        {
            ["requestId"] = requestId,
            ["attachments"] = attachmentUrls ?? new List<string>()
        };

        // Create request
        // Ensure DateTime values are UTC for PostgreSQL compatibility
        var effectiveFrom = DateTime.SpecifyKind(dto.StartDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var effectiveTo = DateTime.SpecifyKind(dto.EndDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var request = new Request
        {
            RequestTypeId = requestTypeLookup.Id,
            RequesterEmployeeId = employeeId,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Reason = dto.Reason,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRequest = await _requestRepository.CreateRequestAsync(request);

        return new TimeOffRequestResponseDto
        {
            Id = requestId,
            Type = requestTypeLookup.Code, // Use uppercase snake_case
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Duration = duration,
            SubmittedDate = createdRequest.CreatedAt,
            Status = createdRequest.Status.ToApiString().ToLower(),
            Reason = createdRequest.Reason,
            Attachments = attachmentUrls,
            Message = "Request submitted successfully"
        };
    }

    public async Task<LeaveBalancesResponseDto> GetLeaveBalancesAsync(long employeeId, int year)
    {
        var currentYear = year == 0 ? DateTime.UtcNow.Year : year;
        var balances = await _leaveBalanceRepository.GetLeaveBalancesAsync(employeeId, currentYear);

        // Ensure all balance types exist with default values
        var balanceTypes = new[] { "Annual Leave", "Sick Leave", "Parental Leave", "Other Leave" };
        var defaultTotals = new Dictionary<string, decimal>
        {
            { "Annual Leave", 15 },
            { "Sick Leave", 10 },
            { "Parental Leave", 14 },
            { "Other Leave", 5 }
        };

        var balanceDict = balances.ToDictionary(b => b.BalanceType, b => b);

        var result = new List<LeaveBalanceDto>();

        foreach (var balanceType in balanceTypes)
        {
            var balance = balanceDict.GetValueOrDefault(balanceType);

            // Initialize balance if it doesn't exist
            if (balance == null)
            {
                var newBalance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    BalanceType = balanceType,
                    Year = currentYear,
                    Total = defaultTotals.GetValueOrDefault(balanceType, 0),
                    Used = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                balance = await _leaveBalanceRepository.CreateOrUpdateLeaveBalanceAsync(newBalance);
            }

            var used = await GetUsedLeaveDaysAsync(employeeId, balanceType, currentYear);
            var total = balance.Total;
            var remaining = total - used;

            result.Add(new LeaveBalanceDto
            {
                Type = balanceType,
                Total = total,
                Used = used,
                Remaining = remaining
            });
        }

        return new LeaveBalancesResponseDto { Balances = result };
    }

    public async Task<TimeOffRequestHistoryResponseDto> GetTimeOffRequestHistoryAsync(
        long employeeId,
        int page = 1,
        int limit = 10,
        string? status = null,
        string? type = null)
    {
        // Map time-off request types
        RequestType? requestTypeFilter = null;
        if (!string.IsNullOrEmpty(type))
        {
            requestTypeFilter = EnumHelper.ParseRequestType(type);
        }

        // Get time-off request types only (as strings)
        var timeOffTypes = new[]
        {
            "PAID_LEAVE",
            "UNPAID_LEAVE",
            "PAID_SICK_LEAVE",
            "UNPAID_SICK_LEAVE",
            "WFH"
        };

        // Normalize filter type if provided
        string? normalizedTypeFilter = null;
        if (!string.IsNullOrEmpty(type))
        {
            normalizedTypeFilter = type.ToUpper().Replace("-", "_");
        }

        var allRequests = await _requestRepository.GetRequestsAsync(
            employeeId,
            status?.ToUpper(),
            normalizedTypeFilter,
            null,
            null,
            1,
            10000); // Get all to filter by type

        // Filter to time-off types only
        var timeOffRequests = allRequests
            .Where(r => r.RequestTypeLookup != null && timeOffTypes.Contains(r.RequestTypeLookup.Code))
            .OrderByDescending(r => r.RequestedAt)
            .ToList();

        // Apply type filter if specified
        if (!string.IsNullOrEmpty(normalizedTypeFilter))
        {
            timeOffRequests = timeOffRequests
                .Where(r => r.RequestTypeLookup != null && r.RequestTypeLookup.Code == normalizedTypeFilter)
                .ToList();
        }

        // Apply pagination
        var total = timeOffRequests.Count;
        var paginated = timeOffRequests
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        var data = paginated.Select(r =>
        {
            var payload = !string.IsNullOrEmpty(r.Payload)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(r.Payload)
                : null;

            var attachments = payload?.ContainsKey("attachments") == true
                ? JsonSerializer.Deserialize<List<string>>(payload["attachments"].ToString() ?? "[]")
                : null;

            var requestId = payload?.ContainsKey("requestId") == true
                ? payload["requestId"].ToString()
                : $"REQ-{r.Id:D3}";

            return new TimeOffRequestHistoryDto
            {
                Id = requestId ?? $"REQ-{r.Id:D3}",
                Type = r.RequestTypeLookup?.Code ?? "UNKNOWN",
                StartDate = r.EffectiveFrom.HasValue
                    ? DateOnly.FromDateTime(r.EffectiveFrom.Value)
                    : DateOnly.MinValue,
                EndDate = r.EffectiveTo.HasValue
                    ? DateOnly.FromDateTime(r.EffectiveTo.Value)
                    : DateOnly.MinValue,
                Duration = r.EffectiveFrom.HasValue && r.EffectiveTo.HasValue
                    ? CalculateDuration(DateOnly.FromDateTime(r.EffectiveFrom.Value), DateOnly.FromDateTime(r.EffectiveTo.Value))
                    : 0,
                SubmittedDate = r.RequestedAt,
                Status = r.Status.ToApiString().ToLower(),
                Reason = r.Reason,
                Attachments = attachments
            };
        }).ToList();

        return new TimeOffRequestHistoryResponseDto
        {
            Data = data,
            Pagination = new PaginationDto
            {
                Page = page,
                Limit = limit,
                Total = total,
                TotalPages = (int)Math.Ceiling((double)total / limit)
            }
        };
    }

    public async Task<TimeOffRequestResponseDto> CancelTimeOffRequestAsync(string requestId, long employeeId, string? comment)
    {
        Request? requestToCancel = null;

        // Parse request ID (format: REQ-XXX)
        var idPart = requestId.Replace("REQ-", "");
        if (int.TryParse(idPart, out var requestDbId))
        {
            requestToCancel = await _requestRepository.GetRequestByIdAsync(requestDbId);
        }

        // If not found by ID, try to find by payload requestId
        if (requestToCancel == null)
        {
            var allRequests = await _requestRepository.GetRequestsAsync(employeeId, null, null, null, null, 1, 1000);
            requestToCancel = allRequests.FirstOrDefault(r =>
            {
                if (string.IsNullOrEmpty(r.Payload)) return false;
                try
                {
                    var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(r.Payload);
                    return payload?.ContainsKey("requestId") == true && payload["requestId"].ToString() == requestId;
                }
                catch
                {
                    return false;
                }
            });
        }

        if (requestToCancel == null)
        {
            throw new KeyNotFoundException($"Time off request with ID {requestId} not found");
        }

        if (requestToCancel.RequesterEmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own requests");
        }

        if (requestToCancel.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be cancelled");
        }

        requestToCancel.Status = RequestStatus.Cancelled;
        if (!string.IsNullOrEmpty(comment))
        {
            requestToCancel.RejectionReason = comment;
        }

        await _requestRepository.UpdateRequestAsync(requestToCancel);

        var payload = !string.IsNullOrEmpty(requestToCancel.Payload)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(requestToCancel.Payload)
            : null;

        var attachments = payload?.ContainsKey("attachments") == true
            ? JsonSerializer.Deserialize<List<string>>(payload["attachments"].ToString() ?? "[]")
            : null;

        return new TimeOffRequestResponseDto
        {
            Id = requestId,
            Type = requestToCancel.RequestTypeLookup?.Code ?? "UNKNOWN",
            StartDate = requestToCancel.EffectiveFrom.HasValue
                ? DateOnly.FromDateTime(requestToCancel.EffectiveFrom.Value)
                : DateOnly.MinValue,
            EndDate = requestToCancel.EffectiveTo.HasValue
                ? DateOnly.FromDateTime(requestToCancel.EffectiveTo.Value)
                : DateOnly.MinValue,
            Duration = requestToCancel.EffectiveFrom.HasValue && requestToCancel.EffectiveTo.HasValue
                ? CalculateDuration(DateOnly.FromDateTime(requestToCancel.EffectiveFrom.Value), DateOnly.FromDateTime(requestToCancel.EffectiveTo.Value))
                : 0,
            SubmittedDate = requestToCancel.RequestedAt,
            Status = requestToCancel.Status.ToApiString().ToLower(),
            Reason = requestToCancel.Reason,
            Attachments = attachments,
            Message = "Request cancelled successfully"
        };
    }

    public async Task<int> CalculateDurationAsync(DateOnly startDate, DateOnly endDate)
    {
        return await Task.FromResult(CalculateDuration(startDate, endDate));
    }

    private int CalculateDuration(DateOnly startDate, DateOnly endDate)
    {
        // Inclusive of both start and end dates
        return endDate.DayNumber - startDate.DayNumber + 1;
    }

    public async Task<string> GenerateRequestIdAsync()
    {
        // Get the latest request to generate next ID
        var allRequests = await _requestRepository.GetRequestsAsync(null, null, null, null, null, 1, 1);
        var latestRequest = allRequests.OrderByDescending(r => r.Id).FirstOrDefault();

        var nextNumber = latestRequest != null ? latestRequest.Id + 1 : 1;
        return $"REQ-{nextNumber:D3}";
    }

    private async Task<decimal> GetUsedLeaveDaysAsync(long employeeId, string balanceType, int year)
    {
        // Map balance type to request types (as strings)
        var requestTypes = balanceType switch
        {
            "Annual Leave" => new[] { "PAID_LEAVE", "UNPAID_LEAVE" },
            "Sick Leave" => new[] { "PAID_SICK_LEAVE", "UNPAID_SICK_LEAVE" },
            _ => Array.Empty<string>()
        };

        if (requestTypes.Length == 0)
        {
            return 0;
        }

        // Get all approved requests for this employee and year
        var allRequests = await _requestRepository.GetRequestsAsync(employeeId, "APPROVED", null, null, null, 1, 10000);

        var relevantRequests = allRequests
            .Where(r => r.RequestTypeLookup != null
                && requestTypes.Contains(r.RequestTypeLookup.Code)
                && r.EffectiveFrom.HasValue
                && r.EffectiveTo.HasValue
                && r.EffectiveFrom.Value.Year == year)
            .ToList();

        decimal totalDays = 0;
        foreach (var request in relevantRequests)
        {
            if (request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue)
            {
                var startDate = DateOnly.FromDateTime(request.EffectiveFrom.Value);
                var endDate = DateOnly.FromDateTime(request.EffectiveTo.Value);
                totalDays += CalculateDuration(startDate, endDate);
            }
        }

        return totalDays;
    }

}

