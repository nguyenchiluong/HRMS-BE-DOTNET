using EmployeeApi.Dtos;

namespace EmployeeApi.Services.Timesheet;

public interface ITimesheetService
{
    // ========================================
    // Timesheet Submission
    // ========================================
    
    /// <summary>
    /// Submit a weekly timesheet
    /// </summary>
    Task<TimesheetResponse> SubmitTimesheetAsync(SubmitTimesheetRequest dto, long employeeId, string? userRole = null);

    /// <summary>
    /// Adjust/update a timesheet (only for pending or rejected)
    /// </summary>
    Task<TimesheetResponse> AdjustTimesheetAsync(int requestId, AdjustTimesheetRequest dto, long employeeId);

    // ========================================
    // Timesheet Queries
    // ========================================
    
    /// <summary>
    /// Get a timesheet by request ID
    /// </summary>
    Task<TimesheetResponse?> GetTimesheetByIdAsync(int requestId);

    /// <summary>
    /// Get my timesheets (for current employee)
    /// </summary>
    Task<PaginatedResponseDto<TimesheetListItem>> GetMyTimesheetsAsync(
        long employeeId,
        int? year = null,
        int? month = null,
        string? status = null,
        int page = 1,
        int limit = 20);

    // ========================================
    // Approval Workflow
    // ========================================
    
    /// <summary>
    /// Get pending timesheet approvals (for manager view)
    /// </summary>
    Task<PaginatedResponseDto<TimesheetApprovalItem>> GetPendingApprovalsAsync(
        long approverEmployeeId,
        int page = 1,
        int limit = 20);

    /// <summary>
    /// Approve a timesheet
    /// </summary>
    Task<TimesheetResponse> ApproveTimesheetAsync(int requestId, long approverId, string? comment);

    /// <summary>
    /// Reject a timesheet
    /// </summary>
    Task<TimesheetResponse> RejectTimesheetAsync(int requestId, long approverId, string reason);

    /// <summary>
    /// Cancel a timesheet (only for pending timesheets)
    /// </summary>
    Task<TimesheetResponse> CancelTimesheetAsync(int requestId, long employeeId);

    // ========================================
    // Task Management
    // ========================================
    
    /// <summary>
    /// Get all active tasks
    /// </summary>
    Task<List<TimesheetTaskResponse>> GetActiveTasksAsync();

    /// <summary>
    /// Get all tasks (including inactive)
    /// </summary>
    Task<List<TimesheetTaskResponse>> GetAllTasksAsync();

    /// <summary>
    /// Create a new task
    /// </summary>
    Task<TimesheetTaskResponse> CreateTaskAsync(CreateTimesheetTaskRequest dto);

    /// <summary>
    /// Update a task
    /// </summary>
    Task<TimesheetTaskResponse> UpdateTaskAsync(int id, UpdateTimesheetTaskRequest dto);
}

