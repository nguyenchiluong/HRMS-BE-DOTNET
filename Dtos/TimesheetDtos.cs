using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Dtos;

// ========================================
// Request DTOs
// ========================================

/// <summary>
/// DTO for submitting a weekly timesheet
/// </summary>
public class SubmitTimesheetRequest
{
    [Required]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(1, 53)]
    public int WeekNumber { get; set; }

    [Required]
    public DateTime WeekStartDate { get; set; }

    [Required]
    public DateTime WeekEndDate { get; set; }

    public string? Reason { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one entry is required")]
    public List<TimesheetEntryInput> Entries { get; set; } = new();
}

/// <summary>
/// Input DTO for a single timesheet entry
/// </summary>
public class TimesheetEntryInput
{
    [Required]
    public int TaskId { get; set; }

    [Required]
    [MaxLength(20)]
    public string EntryType { get; set; } = "project"; // "project" | "leave"

    [Required]
    [Range(0, 168, ErrorMessage = "Hours must be between 0 and 168")]
    public decimal Hours { get; set; }
}

/// <summary>
/// DTO for adjusting/updating a timesheet (only for pending or rejected)
/// </summary>
public class AdjustTimesheetRequest
{
    public string? Reason { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one entry is required")]
    public List<TimesheetEntryInput> Entries { get; set; } = new();
}

// ========================================
// Response DTOs
// ========================================

/// <summary>
/// Response DTO for a timesheet (with request info and entries)
/// </summary>
public class TimesheetResponse
{
    public int RequestId { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string? Department { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WeekNumber { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string Status { get; set; } = default!;
    public string? Reason { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public long? ApproverEmployeeId { get; set; }
    public string? ApproverName { get; set; }
    public string? ApprovalComment { get; set; }
    public string? RejectionReason { get; set; }
    public TimesheetSummary Summary { get; set; } = new();
    public List<TimesheetEntryResponse> Entries { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response DTO for a single timesheet entry
/// </summary>
public class TimesheetEntryResponse
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskCode { get; set; } = default!;
    public string TaskName { get; set; } = default!;
    public string EntryType { get; set; } = default!;
    public decimal Hours { get; set; }
}

/// <summary>
/// Summary of hours in a timesheet
/// </summary>
public class TimesheetSummary
{
    public decimal TotalHours { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal LeaveHours { get; set; }
}

/// <summary>
/// Simplified response for listing timesheets (without entries)
/// </summary>
public class TimesheetListItem
{
    public int RequestId { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string? Department { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WeekNumber { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string Status { get; set; } = default!;
    public TimesheetSummary Summary { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response for pending approvals list (manager view)
/// </summary>
public class TimesheetApprovalItem
{
    public int RequestId { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string? Department { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WeekNumber { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public TimesheetSummary Summary { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = default!;
}

// ========================================
// Task DTOs
// ========================================

/// <summary>
/// Response DTO for a timesheet task
/// </summary>
public class TimesheetTaskResponse
{
    public int Id { get; set; }
    public string TaskCode { get; set; } = default!;
    public string TaskName { get; set; } = default!;
    public string? Description { get; set; }
    public string TaskType { get; set; } = default!;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a new timesheet task
/// </summary>
public class CreateTimesheetTaskRequest
{
    [Required]
    [MaxLength(50)]
    public string TaskCode { get; set; } = default!;

    [Required]
    [MaxLength(255)]
    public string TaskName { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(20)]
    public string TaskType { get; set; } = "project"; // "project" | "leave"
}

/// <summary>
/// DTO for updating a timesheet task
/// </summary>
public class UpdateTimesheetTaskRequest
{
    [MaxLength(255)]
    public string? TaskName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

// ========================================
// Payload DTOs (stored in Request.Payload as JSON)
// ========================================

/// <summary>
/// Payload stored in Request.Payload for timesheet requests
/// </summary>
public class TimesheetPayload
{
    public string PeriodType { get; set; } = "weekly";
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public int WeekNumber { get; set; }
    public TimesheetSummary Summary { get; set; } = new();
}

