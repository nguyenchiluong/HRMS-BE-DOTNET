namespace EmployeeApi.Dtos;

/// <summary>
/// DTO for team member with time management data
/// </summary>
public class TeamMemberDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = default!;
    public string WorkEmail { get; set; } = default!;
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string? JobLevel { get; set; }
    public string? Status { get; set; }
    public string? EmploymentType { get; set; }
    public string? TimeType { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public DateOnly? StartDate { get; set; }
    
    // Time Management Fields
    public string? AttendanceStatus { get; set; } // "clocked-in" | "clocked-out"
    public string? ClockInTime { get; set; } // ISO timestamp (null if not clocked in)
    public int? CurrentWorkingMinutes { get; set; } // Minutes worked today (null if clocked out)
    public int PendingTimesheetCount { get; set; }
    public int PendingTimeOffCount { get; set; }
    public string? LastTimesheetStatus { get; set; } // "DRAFT" | "PENDING" | "APPROVED" | "REJECTED" | "CANCELLED" | null
}

/// <summary>
/// Summary metrics for team members
/// </summary>
public class TeamMembersSummaryDto
{
    public int ActiveMembers { get; set; }
    public int ClockedInCount { get; set; }
    public int TotalPendingTimesheets { get; set; }
    public int TotalPendingTimeOff { get; set; }
}

/// <summary>
/// Paginated response for team members
/// </summary>
public class TeamMembersResponseDto
{
    public List<TeamMemberDto> TeamMembers { get; set; } = new();
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
