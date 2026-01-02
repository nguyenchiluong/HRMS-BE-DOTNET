using System.Globalization;
using System.Text.Json;
using EmployeeApi.Dtos;
using EmployeeApi.Helpers;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services.Timesheet;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRequestTypeRepository _requestTypeRepository;
    private readonly ILogger<TimesheetService> _logger;

    private const decimal MAX_HOURS_PER_WEEK = 168m; // 7 days * 24 hours
    private const decimal REGULAR_HOURS_PER_WEEK = 40m;

    public TimesheetService(
        ITimesheetRepository timesheetRepository,
        IRequestRepository requestRepository,
        IEmployeeRepository employeeRepository,
        IRequestTypeRepository requestTypeRepository,
        ILogger<TimesheetService> logger)
    {
        _timesheetRepository = timesheetRepository;
        _requestRepository = requestRepository;
        _employeeRepository = employeeRepository;
        _requestTypeRepository = requestTypeRepository;
        _logger = logger;
    }

    // ========================================
    // Timesheet Submission
    // ========================================

    public async Task<TimesheetResponse> SubmitTimesheetAsync(SubmitTimesheetRequest dto, long employeeId)
    {
        var weekStartDate = DateOnly.FromDateTime(dto.WeekStartDate);
        var weekEndDate = DateOnly.FromDateTime(dto.WeekEndDate);

        // 1. Validate: no duplicate submission for same week (unless cancelled)
        if (await _timesheetRepository.ExistsForWeekAsync(employeeId, weekStartDate))
        {
            var existingRequestId = await _timesheetRepository.GetRequestIdForWeekAsync(employeeId, weekStartDate);

            if (existingRequestId.HasValue)
            {
                var existingRequest = await _requestRepository.GetRequestByIdAsync(existingRequestId.Value);

                // Allow resubmission if the existing timesheet is CANCELLED
                if (existingRequest != null && existingRequest.Status == RequestStatus.Cancelled)
                {
                    // Delete old entries from the cancelled timesheet to allow clean resubmission
                    await _timesheetRepository.DeleteEntriesByRequestIdAsync(existingRequestId.Value);
                }
                else
                {
                    // Block resubmission if timesheet exists and is not cancelled
                    throw new InvalidOperationException(
                        $"A timesheet already exists for this week. Request ID: {existingRequestId}");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"A timesheet already exists for this week. Request ID: {existingRequestId}");
            }
        }

        // 2. Validate: total hours per week <= MAX_HOURS_PER_WEEK
        var totalHours = dto.Entries.Sum(e => e.Hours);
        if (totalHours > MAX_HOURS_PER_WEEK)
        {
            throw new InvalidOperationException(
                $"Total hours ({totalHours}) exceeds maximum allowed hours per week ({MAX_HOURS_PER_WEEK})");
        }

        // 3. Validate: all task IDs exist
        foreach (var entry in dto.Entries)
        {
            var task = await _timesheetRepository.GetTaskByIdAsync(entry.TaskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task with ID {entry.TaskId} not found");
            }
            if (!task.IsActive)
            {
                throw new InvalidOperationException($"Task '{task.TaskCode}' is not active");
            }
        }

        // 4. Calculate summary
        var summary = CalculateSummary(dto.Entries);

        // 5. Find approver (manager) from employee hierarchy
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException("Employee not found");
        }

        // 6. Create request record
        // Ensure DateTime values are UTC for PostgreSQL compatibility
        var effectiveFrom = dto.WeekStartDate.Kind == DateTimeKind.Utc
            ? dto.WeekStartDate
            : DateTime.SpecifyKind(dto.WeekStartDate.Date, DateTimeKind.Utc);
        var effectiveTo = dto.WeekEndDate.Kind == DateTimeKind.Utc
            ? dto.WeekEndDate
            : DateTime.SpecifyKind(dto.WeekEndDate.Date, DateTimeKind.Utc);

        // Look up timesheet request type
        var timesheetRequestType = await _requestTypeRepository.GetRequestTypeByCodeAsync("TIMESHEET_WEEKLY");
        if (timesheetRequestType == null)
        {
            throw new InvalidOperationException("Timesheet request type not found in database");
        }

        var request = new Request
        {
            RequestTypeId = timesheetRequestType.Id,
            RequesterEmployeeId = employeeId,
            ApproverEmployeeId = employee.ManagerId,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Reason = dto.Reason ?? $"Weekly timesheet for Week {dto.WeekNumber}, {dto.Month}/{dto.Year}",
            Payload = JsonSerializer.Serialize(new TimesheetPayload
            {
                PeriodType = "weekly",
                PeriodYear = dto.Year,
                PeriodMonth = dto.Month,
                WeekNumber = dto.WeekNumber,
                Summary = summary
            }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRequest = await _requestRepository.CreateRequestAsync(request);

        // 7. Bulk insert timesheet_entry records
        var entries = dto.Entries.Select(e => new TimesheetEntry
        {
            RequestId = createdRequest.Id,
            EmployeeId = employeeId,
            TaskId = e.TaskId,
            EntryType = e.EntryType,
            WeekStartDate = weekStartDate,
            WeekEndDate = weekEndDate,
            Hours = e.Hours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _timesheetRepository.CreateEntriesAsync(entries);

        // 8. Return response
        return await BuildTimesheetResponseAsync(createdRequest.Id);
    }

    public async Task<TimesheetResponse> AdjustTimesheetAsync(int requestId, AdjustTimesheetRequest dto, long employeeId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null)
        {
            throw new InvalidOperationException("Timesheet request not found");
        }

        // Verify ownership
        if (request.RequesterEmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only adjust your own timesheets");
        }

        // Only allow adjustments for pending or rejected timesheets
        if (request.Status != RequestStatus.Pending && request.Status != RequestStatus.Rejected)
        {
            throw new InvalidOperationException("Only PENDING or REJECTED timesheets can be adjusted");
        }

        // Validate hours
        var totalHours = dto.Entries.Sum(e => e.Hours);
        if (totalHours > MAX_HOURS_PER_WEEK)
        {
            throw new InvalidOperationException(
                $"Total hours ({totalHours}) exceeds maximum allowed hours per week ({MAX_HOURS_PER_WEEK})");
        }

        // Validate tasks
        foreach (var entry in dto.Entries)
        {
            var task = await _timesheetRepository.GetTaskByIdAsync(entry.TaskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task with ID {entry.TaskId} not found");
            }
        }

        // Delete existing entries
        await _timesheetRepository.DeleteEntriesByRequestIdAsync(requestId);

        // Get week dates from the request
        var weekStartDate = DateOnly.FromDateTime(request.EffectiveFrom!.Value);
        var weekEndDate = DateOnly.FromDateTime(request.EffectiveTo!.Value);

        // Create new entries
        var entries = dto.Entries.Select(e => new TimesheetEntry
        {
            RequestId = requestId,
            EmployeeId = employeeId,
            TaskId = e.TaskId,
            EntryType = e.EntryType,
            WeekStartDate = weekStartDate,
            WeekEndDate = weekEndDate,
            Hours = e.Hours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _timesheetRepository.CreateEntriesAsync(entries);

        // Update summary in payload
        var summary = CalculateSummary(dto.Entries);
        var payload = JsonSerializer.Deserialize<TimesheetPayload>(request.Payload ?? "{}") ?? new TimesheetPayload();
        payload.Summary = summary;
        request.Payload = JsonSerializer.Serialize(payload);

        // Update reason if provided
        if (!string.IsNullOrEmpty(dto.Reason))
        {
            request.Reason = dto.Reason;
        }

        // Reset status to pending if it was rejected
        if (request.Status == RequestStatus.Rejected)
        {
            request.Status = RequestStatus.Pending;
            request.RejectionReason = null;
        }

        await _requestRepository.UpdateRequestAsync(request);

        return await BuildTimesheetResponseAsync(requestId);
    }

    // ========================================
    // Timesheet Queries
    // ========================================

    public async Task<TimesheetResponse?> GetTimesheetByIdAsync(int requestId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.RequestTypeLookup?.Code != "TIMESHEET_WEEKLY")
        {
            return null;
        }

        return await BuildTimesheetResponseAsync(requestId);
    }

    public async Task<PaginatedResponseDto<TimesheetListItem>> GetMyTimesheetsAsync(
        long employeeId,
        int? year = null,
        int? month = null,
        string? status = null,
        int page = 1,
        int limit = 20)
    {
        var requests = await _timesheetRepository.GetTimesheetRequestsAsync(
            employeeId, year, month, status, page, limit);

        var totalCount = await _timesheetRepository.GetTimesheetRequestsCountAsync(
            employeeId, year, month, status);

        var items = requests.Select(MapToTimesheetListItem).ToList();

        return new PaginatedResponseDto<TimesheetListItem>
        {
            Data = items,
            Pagination = new PaginationDto
            {
                Page = page,
                Limit = limit,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            }
        };
    }

    // ========================================
    // Approval Workflow
    // ========================================

    public async Task<PaginatedResponseDto<TimesheetApprovalItem>> GetPendingApprovalsAsync(
        long approverEmployeeId,
        int page = 1,
        int limit = 20)
    {
        var requests = await _timesheetRepository.GetPendingApprovalsAsync(
            approverEmployeeId, null, page, limit);

        var totalCount = await _timesheetRepository.GetPendingApprovalsCountAsync(
            approverEmployeeId, null);

        var items = requests.Select(MapToTimesheetApprovalItem).ToList();

        return new PaginatedResponseDto<TimesheetApprovalItem>
        {
            Data = items,
            Pagination = new PaginationDto
            {
                Page = page,
                Limit = limit,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            }
        };
    }

    public async Task<TimesheetResponse> ApproveTimesheetAsync(int requestId, long approverId, string? comment)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.RequestTypeLookup?.Code != "TIMESHEET_WEEKLY")
        {
            throw new InvalidOperationException("Timesheet request not found");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING timesheets can be approved");
        }

        request.Status = RequestStatus.Approved;
        request.ApproverEmployeeId = approverId;
        request.ApprovalComment = comment;
        request.UpdatedAt = DateTime.UtcNow;

        await _requestRepository.UpdateRequestAsync(request);

        return await BuildTimesheetResponseAsync(requestId);
    }

    public async Task<TimesheetResponse> RejectTimesheetAsync(int requestId, long approverId, string reason)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.RequestTypeLookup?.Code != "TIMESHEET_WEEKLY")
        {
            throw new InvalidOperationException("Timesheet request not found");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING timesheets can be rejected");
        }

        request.Status = RequestStatus.Rejected;
        request.ApproverEmployeeId = approverId;
        request.RejectionReason = reason;
        request.UpdatedAt = DateTime.UtcNow;

        await _requestRepository.UpdateRequestAsync(request);

        return await BuildTimesheetResponseAsync(requestId);
    }

    public async Task<TimesheetResponse> CancelTimesheetAsync(int requestId, long employeeId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null || request.RequestTypeLookup?.Code != "TIMESHEET_WEEKLY")
        {
            throw new InvalidOperationException("Timesheet request not found");
        }

        // Verify ownership
        if (request.RequesterEmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own timesheets");
        }

        // Only allow cancellation if status is PENDING
        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING timesheets can be cancelled");
        }

        request.Status = RequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _requestRepository.UpdateRequestAsync(request);

        return await BuildTimesheetResponseAsync(requestId);
    }

    // ========================================
    // Task Management
    // ========================================

    public async Task<List<TimesheetTaskResponse>> GetActiveTasksAsync()
    {
        var tasks = await _timesheetRepository.GetActiveTasksAsync();
        return tasks.Select(MapToTimesheetTaskResponse).ToList();
    }

    public async Task<List<TimesheetTaskResponse>> GetAllTasksAsync()
    {
        var tasks = await _timesheetRepository.GetAllTasksAsync();
        return tasks.Select(MapToTimesheetTaskResponse).ToList();
    }

    public async Task<TimesheetTaskResponse> CreateTaskAsync(CreateTimesheetTaskRequest dto)
    {
        // Check for duplicate task code
        var existingTask = await _timesheetRepository.GetTaskByCodeAsync(dto.TaskCode);
        if (existingTask != null)
        {
            throw new InvalidOperationException($"Task with code '{dto.TaskCode}' already exists");
        }

        var task = new TimesheetTask
        {
            TaskCode = dto.TaskCode,
            TaskName = dto.TaskName,
            Description = dto.Description,
            TaskType = dto.TaskType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdTask = await _timesheetRepository.CreateTaskAsync(task);
        return MapToTimesheetTaskResponse(createdTask);
    }

    public async Task<TimesheetTaskResponse> UpdateTaskAsync(int id, UpdateTimesheetTaskRequest dto)
    {
        var task = await _timesheetRepository.GetTaskByIdAsync(id);
        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        if (!string.IsNullOrEmpty(dto.TaskName))
        {
            task.TaskName = dto.TaskName;
        }

        if (dto.Description != null)
        {
            task.Description = dto.Description;
        }

        if (dto.IsActive.HasValue)
        {
            task.IsActive = dto.IsActive.Value;
        }

        var updatedTask = await _timesheetRepository.UpdateTaskAsync(task);
        return MapToTimesheetTaskResponse(updatedTask);
    }

    // ========================================
    // Private Helpers
    // ========================================

    private TimesheetSummary CalculateSummary(List<TimesheetEntryInput> entries)
    {
        var projectHours = entries.Where(e => e.EntryType == "project").Sum(e => e.Hours);
        var leaveHours = entries.Where(e => e.EntryType == "leave").Sum(e => e.Hours);
        var totalHours = projectHours + leaveHours;

        var regularHours = Math.Min(projectHours, REGULAR_HOURS_PER_WEEK);
        var overtimeHours = Math.Max(0, projectHours - REGULAR_HOURS_PER_WEEK);

        return new TimesheetSummary
        {
            TotalHours = totalHours,
            RegularHours = regularHours,
            OvertimeHours = overtimeHours,
            LeaveHours = leaveHours
        };
    }

    private async Task<TimesheetResponse> BuildTimesheetResponseAsync(int requestId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(requestId);
        if (request == null)
        {
            throw new InvalidOperationException("Request not found");
        }

        var entries = await _timesheetRepository.GetEntriesByRequestIdAsync(requestId);

        var payload = !string.IsNullOrEmpty(request.Payload)
            ? JsonSerializer.Deserialize<TimesheetPayload>(request.Payload)
            : null;

        return new TimesheetResponse
        {
            RequestId = request.Id,
            EmployeeId = request.RequesterEmployeeId,
            EmployeeName = request.Requester?.FullName ?? "",
            Department = request.Requester?.Department?.Name,
            Year = payload?.PeriodYear ?? 0,
            Month = payload?.PeriodMonth ?? 0,
            WeekNumber = payload?.WeekNumber ?? 0,
            WeekStartDate = request.EffectiveFrom ?? DateTime.MinValue,
            WeekEndDate = request.EffectiveTo ?? DateTime.MinValue,
            Status = request.Status.ToApiString(),
            Reason = request.Reason,
            SubmittedAt = request.RequestedAt,
            ApprovedAt = request.Status == RequestStatus.Approved ? request.UpdatedAt : null,
            ApproverEmployeeId = request.ApproverEmployeeId,
            ApproverName = request.Approver?.FullName,
            ApprovalComment = request.ApprovalComment,
            RejectionReason = request.RejectionReason,
            Summary = payload?.Summary ?? new TimesheetSummary(),
            Entries = entries.Select(e => new TimesheetEntryResponse
            {
                Id = e.Id,
                TaskId = e.TaskId,
                TaskCode = e.Task?.TaskCode ?? "",
                TaskName = e.Task?.TaskName ?? "",
                EntryType = e.EntryType,
                Hours = e.Hours
            }).ToList(),
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
    }

    private TimesheetListItem MapToTimesheetListItem(Request request)
    {
        var payload = !string.IsNullOrEmpty(request.Payload)
            ? JsonSerializer.Deserialize<TimesheetPayload>(request.Payload)
            : null;

        return new TimesheetListItem
        {
            RequestId = request.Id,
            EmployeeId = request.RequesterEmployeeId,
            EmployeeName = request.Requester?.FullName ?? "",
            Department = request.Requester?.Department?.Name,
            Year = payload?.PeriodYear ?? 0,
            Month = payload?.PeriodMonth ?? 0,
            WeekNumber = payload?.WeekNumber ?? 0,
            WeekStartDate = request.EffectiveFrom ?? DateTime.MinValue,
            WeekEndDate = request.EffectiveTo ?? DateTime.MinValue,
            Status = request.Status.ToApiString(),
            Summary = payload?.Summary ?? new TimesheetSummary(),
            SubmittedAt = request.RequestedAt,
            CreatedAt = request.CreatedAt
        };
    }

    private TimesheetApprovalItem MapToTimesheetApprovalItem(Request request)
    {
        var payload = !string.IsNullOrEmpty(request.Payload)
            ? JsonSerializer.Deserialize<TimesheetPayload>(request.Payload)
            : null;

        return new TimesheetApprovalItem
        {
            RequestId = request.Id,
            EmployeeId = request.RequesterEmployeeId,
            EmployeeName = request.Requester?.FullName ?? "",
            Department = request.Requester?.Department?.Name,
            Year = payload?.PeriodYear ?? 0,
            Month = payload?.PeriodMonth ?? 0,
            WeekNumber = payload?.WeekNumber ?? 0,
            WeekStartDate = request.EffectiveFrom ?? DateTime.MinValue,
            WeekEndDate = request.EffectiveTo ?? DateTime.MinValue,
            Summary = payload?.Summary ?? new TimesheetSummary(),
            SubmittedAt = request.RequestedAt,
            Status = request.Status.ToApiString()
        };
    }

    private TimesheetTaskResponse MapToTimesheetTaskResponse(TimesheetTask task)
    {
        return new TimesheetTaskResponse
        {
            Id = task.Id,
            TaskCode = task.TaskCode,
            TaskName = task.TaskName,
            Description = task.Description,
            TaskType = task.TaskType,
            IsActive = task.IsActive
        };
    }
}

