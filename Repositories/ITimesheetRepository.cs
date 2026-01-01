using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface ITimesheetRepository
{
    // ========================================
    // Timesheet Entry Operations
    // ========================================
    
    /// <summary>
    /// Get timesheet entries by request ID
    /// </summary>
    Task<List<TimesheetEntry>> GetEntriesByRequestIdAsync(int requestId);

    /// <summary>
    /// Get timesheet entries for an employee within a date range
    /// </summary>
    Task<List<TimesheetEntry>> GetEntriesByEmployeeAndDateRangeAsync(
        long employeeId,
        DateOnly startDate,
        DateOnly endDate);

    /// <summary>
    /// Create multiple timesheet entries
    /// </summary>
    Task<List<TimesheetEntry>> CreateEntriesAsync(List<TimesheetEntry> entries);

    /// <summary>
    /// Update multiple timesheet entries
    /// </summary>
    Task<List<TimesheetEntry>> UpdateEntriesAsync(List<TimesheetEntry> entries);

    /// <summary>
    /// Delete entries by request ID
    /// </summary>
    Task DeleteEntriesByRequestIdAsync(int requestId);

    /// <summary>
    /// Check if a timesheet already exists for a given employee and week
    /// </summary>
    Task<bool> ExistsForWeekAsync(long employeeId, DateOnly weekStartDate);

    /// <summary>
    /// Get the request ID for an existing timesheet week
    /// </summary>
    Task<int?> GetRequestIdForWeekAsync(long employeeId, DateOnly weekStartDate);

    // ========================================
    // Timesheet Task Operations
    // ========================================
    
    /// <summary>
    /// Get all active timesheet tasks
    /// </summary>
    Task<List<TimesheetTask>> GetActiveTasksAsync();

    /// <summary>
    /// Get all timesheet tasks (including inactive)
    /// </summary>
    Task<List<TimesheetTask>> GetAllTasksAsync();

    /// <summary>
    /// Get a task by ID
    /// </summary>
    Task<TimesheetTask?> GetTaskByIdAsync(int id);

    /// <summary>
    /// Get a task by code
    /// </summary>
    Task<TimesheetTask?> GetTaskByCodeAsync(string taskCode);

    /// <summary>
    /// Create a new timesheet task
    /// </summary>
    Task<TimesheetTask> CreateTaskAsync(TimesheetTask task);

    /// <summary>
    /// Update a timesheet task
    /// </summary>
    Task<TimesheetTask> UpdateTaskAsync(TimesheetTask task);

    // ========================================
    // Query Operations for Reporting
    // ========================================
    
    /// <summary>
    /// Get requests with entries for an employee within a month
    /// </summary>
    Task<List<Request>> GetTimesheetRequestsAsync(
        long? employeeId = null,
        int? year = null,
        int? month = null,
        string? status = null,
        int page = 1,
        int limit = 20);

    /// <summary>
    /// Get count of timesheet requests matching the criteria
    /// </summary>
    Task<int> GetTimesheetRequestsCountAsync(
        long? employeeId = null,
        int? year = null,
        int? month = null,
        string? status = null);

    /// <summary>
    /// Get pending timesheet requests for approval (for manager view)
    /// </summary>
    Task<List<Request>> GetPendingApprovalsAsync(
        long? approverEmployeeId = null,
        long? departmentId = null,
        int page = 1,
        int limit = 20);

    /// <summary>
    /// Get count of pending approvals
    /// </summary>
    Task<int> GetPendingApprovalsCountAsync(
        long? approverEmployeeId = null,
        long? departmentId = null);
}

