namespace EmployeeApi.Dtos;

/// <summary>
/// Employee dashboard statistics DTO
/// </summary>
public record EmployeeDashboardStatsDto(
    /// <summary>
    /// Current bonus credit points balance (placeholder for future integration)
    /// </summary>
    decimal BonusBalance,

    /// <summary>
    /// Count of timesheets with status "PENDING"
    /// </summary>
    int PendingTimesheets,

    /// <summary>
    /// Sum of approved timesheet hours for current month
    /// </summary>
    decimal TotalHoursThisMonth,

    /// <summary>
    /// Remaining leave days for the year
    /// </summary>
    decimal LeaveBalance
);

/// <summary>
/// Admin dashboard statistics DTO
/// </summary>
public record AdminDashboardStatsDto(
    /// <summary>
    /// Total count of employees
    /// </summary>
    int TotalEmployees,

    /// <summary>
    /// Count of employees with status "active"
    /// </summary>
    int ActiveEmployees,

    /// <summary>
    /// Count of distinct departments
    /// </summary>
    int Departments,

    /// <summary>
    /// Sum of Position.Salary for all employees with positions
    /// </summary>
    decimal TotalPayroll,

    /// <summary>
    /// Pending approval counts
    /// </summary>
    PendingApprovalsDto PendingApprovals
);

/// <summary>
/// Pending approvals breakdown DTO
/// </summary>
public record PendingApprovalsDto(
    /// <summary>
    /// Count of pending timesheet requests
    /// </summary>
    int Timesheets,

    /// <summary>
    /// Count of pending time-off requests
    /// </summary>
    int TimeOff
);
