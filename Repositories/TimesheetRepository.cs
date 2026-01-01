using EmployeeApi.Data;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Repositories;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly AppDbContext _context;

    public TimesheetRepository(AppDbContext context)
    {
        _context = context;
    }

    // ========================================
    // Timesheet Entry Operations
    // ========================================

    public async Task<List<TimesheetEntry>> GetEntriesByRequestIdAsync(int requestId)
    {
        return await _context.TimesheetEntries
            .Include(te => te.Task)
            .Where(te => te.RequestId == requestId)
            .OrderBy(te => te.Task!.TaskCode)
            .ToListAsync();
    }

    public async Task<List<TimesheetEntry>> GetEntriesByEmployeeAndDateRangeAsync(
        long employeeId,
        DateOnly startDate,
        DateOnly endDate)
    {
        return await _context.TimesheetEntries
            .Include(te => te.Task)
            .Include(te => te.Request)
            .Where(te => te.EmployeeId == employeeId
                && te.WeekStartDate >= startDate
                && te.WeekEndDate <= endDate)
            .OrderBy(te => te.WeekStartDate)
            .ThenBy(te => te.Task!.TaskCode)
            .ToListAsync();
    }

    public async Task<List<TimesheetEntry>> CreateEntriesAsync(List<TimesheetEntry> entries)
    {
        _context.TimesheetEntries.AddRange(entries);
        await _context.SaveChangesAsync();
        return entries;
    }

    public async Task<List<TimesheetEntry>> UpdateEntriesAsync(List<TimesheetEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.UpdatedAt = DateTime.UtcNow;
        }
        _context.TimesheetEntries.UpdateRange(entries);
        await _context.SaveChangesAsync();
        return entries;
    }

    public async Task DeleteEntriesByRequestIdAsync(int requestId)
    {
        var entries = await _context.TimesheetEntries
            .Where(te => te.RequestId == requestId)
            .ToListAsync();

        _context.TimesheetEntries.RemoveRange(entries);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForWeekAsync(long employeeId, DateOnly weekStartDate)
    {
        return await _context.TimesheetEntries
            .AnyAsync(te => te.EmployeeId == employeeId && te.WeekStartDate == weekStartDate);
    }

    public async Task<int?> GetRequestIdForWeekAsync(long employeeId, DateOnly weekStartDate)
    {
        var entry = await _context.TimesheetEntries
            .Where(te => te.EmployeeId == employeeId && te.WeekStartDate == weekStartDate)
            .FirstOrDefaultAsync();

        return entry?.RequestId;
    }

    // ========================================
    // Timesheet Task Operations
    // ========================================

    public async Task<List<TimesheetTask>> GetActiveTasksAsync()
    {
        return await _context.TimesheetTasks
            .Where(t => t.IsActive)
            .OrderBy(t => t.TaskCode)
            .ToListAsync();
    }

    public async Task<List<TimesheetTask>> GetAllTasksAsync()
    {
        return await _context.TimesheetTasks
            .OrderBy(t => t.TaskCode)
            .ToListAsync();
    }

    public async Task<TimesheetTask?> GetTaskByIdAsync(int id)
    {
        return await _context.TimesheetTasks.FindAsync(id);
    }

    public async Task<TimesheetTask?> GetTaskByCodeAsync(string taskCode)
    {
        return await _context.TimesheetTasks
            .FirstOrDefaultAsync(t => t.TaskCode == taskCode);
    }

    public async Task<TimesheetTask> CreateTaskAsync(TimesheetTask task)
    {
        _context.TimesheetTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TimesheetTask> UpdateTaskAsync(TimesheetTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.TimesheetTasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    // ========================================
    // Query Operations for Reporting
    // ========================================

    public async Task<List<Request>> GetTimesheetRequestsAsync(
        long? employeeId = null,
        int? year = null,
        int? month = null,
        string? status = null,
        int page = 1,
        int limit = 20)
    {
        var query = BuildTimesheetQuery(employeeId, year, month, status);

        return await query
            .Include(r => r.Requester)
                .ThenInclude(e => e!.Department)
            .Include(r => r.Approver)
            .Include(r => r.TimesheetEntries!)
                .ThenInclude(te => te.Task)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetTimesheetRequestsCountAsync(
        long? employeeId = null,
        int? year = null,
        int? month = null,
        string? status = null)
    {
        var query = BuildTimesheetQuery(employeeId, year, month, status);
        return await query.CountAsync();
    }

    public async Task<List<Request>> GetPendingApprovalsAsync(
        long? approverEmployeeId = null,
        long? departmentId = null,
        int page = 1,
        int limit = 20)
    {
        var query = _context.Requests
            .Where(r => r.RequestType == RequestType.TimesheetWeekly
                && r.Status == RequestStatus.Pending);

        if (approverEmployeeId.HasValue)
        {
            // Get employees who report to this approver
            var managedEmployeeIds = await _context.Employees
                .Where(e => e.ManagerId == approverEmployeeId.Value)
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(r => managedEmployeeIds.Contains(r.RequesterEmployeeId));
        }

        if (departmentId.HasValue)
        {
            var departmentEmployeeIds = await _context.Employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(r => departmentEmployeeIds.Contains(r.RequesterEmployeeId));
        }

        return await query
            .Include(r => r.Requester)
                .ThenInclude(e => e!.Department)
            .Include(r => r.TimesheetEntries!)
                .ThenInclude(te => te.Task)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetPendingApprovalsCountAsync(
        long? approverEmployeeId = null,
        long? departmentId = null)
    {
        var query = _context.Requests
            .Where(r => r.RequestType == RequestType.TimesheetWeekly
                && r.Status == RequestStatus.Pending);

        if (approverEmployeeId.HasValue)
        {
            var managedEmployeeIds = await _context.Employees
                .Where(e => e.ManagerId == approverEmployeeId.Value)
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(r => managedEmployeeIds.Contains(r.RequesterEmployeeId));
        }

        if (departmentId.HasValue)
        {
            var departmentEmployeeIds = await _context.Employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(r => departmentEmployeeIds.Contains(r.RequesterEmployeeId));
        }

        return await query.CountAsync();
    }

    // ========================================
    // Private Helpers
    // ========================================

    private IQueryable<Request> BuildTimesheetQuery(
        long? employeeId,
        int? year,
        int? month,
        string? status)
    {
        var query = _context.Requests
            .Where(r => r.RequestType == RequestType.TimesheetWeekly);

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        if (year.HasValue && month.HasValue)
        {
            var startOfMonth = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value), 23, 59, 59, 999, DateTimeKind.Utc);
            // Include timesheets where the week overlaps with the requested month
            // A week overlaps if: week starts before/on month end AND week ends on/after month start
            query = query.Where(r => r.EffectiveFrom <= endOfMonth && r.EffectiveTo >= startOfMonth);
        }
        else if (year.HasValue)
        {
            var startOfYear = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = new DateTime(year.Value, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
            // Include timesheets where the week overlaps with the requested year
            query = query.Where(r => r.EffectiveFrom <= endOfYear && r.EffectiveTo >= startOfYear);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<RequestStatus>(status, ignoreCase: true);
            query = query.Where(r => r.Status == statusEnum);
        }

        return query;
    }
}

