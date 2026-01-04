using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EmployeeApi.Dtos;

// Request DTOs
public class CreateRequestDto
{
    [Required]
    public string RequestType { get; set; } = default!; // LEAVE, SICK_LEAVE, WFH, TIMESHEET, PROFILE_UPDATE, ID_UPDATE

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [Required]
    [MinLength(10)]
    public string Reason { get; set; } = default!;

    public JsonElement? Payload { get; set; }
}

public class UpdateRequestDto
{
    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string? Reason { get; set; }

    public JsonElement? Payload { get; set; }
}

public class RequestDto
{
    public string Id { get; set; } = default!; // String format for frontend (e.g., "REQ-001" or numeric as string)
    public string Type { get; set; } = default!; // Request type (PAID_LEAVE, TIMESHEET_WEEKLY, etc.)
    public string EmployeeId { get; set; } = default!; // String format
    public string? EmployeeName { get; set; }
    public string? EmployeeEmail { get; set; }
    public string? EmployeeAvatar { get; set; }
    public string? Department { get; set; }
    public string Status { get; set; } = default!;
    public string? StartDate { get; set; } // yyyy-MM-dd format for time-off requests
    public string? EndDate { get; set; } // yyyy-MM-dd format for time-off requests
    public int? Duration { get; set; } // Number of days (for time-off requests)
    public string SubmittedDate { get; set; } = default!; // ISO timestamp
    public string Reason { get; set; } = default!;
    public List<string>? Attachments { get; set; }
}

public class RequestDetailsDto
{
    public int Id { get; set; }
    public string RequestType { get; set; } = default!;
    public long RequesterEmployeeId { get; set; }
    public long? ApproverEmployeeId { get; set; }
    public string Status { get; set; } = default!;
    public DateTime RequestedAt { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Reason { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public EmployeeSummaryDto? Requester { get; set; }
    public EmployeeSummaryDto? Approver { get; set; }
    public JsonElement? Payload { get; set; }
    public string? ApprovalComment { get; set; }
    public string? RejectionReason { get; set; }
}

public class EmployeeSummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Email { get; set; }
    public string? Department { get; set; }
}

public class ApprovalDto
{
    public string? Comment { get; set; }
}

public class RejectionDto
{
    [Required]
    [MinLength(10)]
    public string Reason { get; set; } = default!;
}

public class RequestsSummaryDto
{
    public int Total { get; set; }
    public StatusCountDto ByStatus { get; set; } = new();
    public Dictionary<string, int> ByType { get; set; } = new();
}

public class StatusCountDto
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}

// Time-Off Request DTOs
public class SubmitTimeOffRequestDto
{
    [Required]
    public string Type { get; set; } = default!; // "PAID_LEAVE", "UNPAID_LEAVE", "PAID_SICK_LEAVE", "UNPAID_SICK_LEAVE", "WFH"

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [MinLength(10)]
    public string Reason { get; set; } = default!;

    public List<string>? Attachments { get; set; } // Array of CloudFront URLs
}

public class TimeOffRequestResponseDto
{
    public string Id { get; set; } = default!; // "REQ-001"
    public string Type { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Duration { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string Status { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public List<string>? Attachments { get; set; }
    public string? Message { get; set; }
}

public class LeaveBalanceDto
{
    public string Type { get; set; } = default!; // "Annual Leave", "Sick Leave", "Parental Leave", "Other Leave"
    public decimal Total { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining { get; set; }
}

public class LeaveBalancesResponseDto
{
    public List<LeaveBalanceDto> Balances { get; set; } = new();
}

public class TimeOffRequestHistoryDto
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Duration { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string Status { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public List<string>? Attachments { get; set; }
}

public class TimeOffRequestHistoryResponseDto
{
    public List<TimeOffRequestHistoryDto> Data { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class CancelTimeOffRequestDto
{
    public string? Comment { get; set; }
}

// Pagination DTOs
public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}
