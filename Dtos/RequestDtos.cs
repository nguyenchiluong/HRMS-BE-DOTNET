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

// Attendance DTOs
public class CheckInDto
{
    public LocationDto? Location { get; set; }
}

public class LocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CheckInResponseDto
{
    public string Message { get; set; } = default!;
    public DateTime CheckInTime { get; set; }
}

public class CheckOutResponseDto
{
    public string Message { get; set; } = default!;
    public DateTime CheckOutTime { get; set; }
    public double TotalHours { get; set; }
}

public class AttendanceRecordDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? TotalHours { get; set; }
    public LocationDto? Location { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
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
