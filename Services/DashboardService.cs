using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Dtos;
using EmployeeApi.Models.Enums;

namespace EmployeeApi.Services;

/// <summary>
/// Implementation of dashboard statistics service
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<EmployeeDashboardStatsDto> GetEmployeeDashboardStatsAsync(long employeeId)
    {
        // Get pending timesheets count for this employee
        // Timesheets are linked to requests via TimesheetEntry.RequestId
        var pendingTimesheets = await _context.Requests
            .Where(r => r.RequesterEmployeeId == employeeId
                && r.Status == RequestStatus.Pending
                && r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category == "timesheet")
            .CountAsync();

        // Get total hours for current month from approved timesheets
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;
        var firstDayOfMonth = new DateOnly(currentYear, currentMonth, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var totalHoursThisMonth = await _context.TimesheetEntries
            .Where(te => te.EmployeeId == employeeId
                && te.Request != null
                && te.Request.Status == RequestStatus.Approved
                && te.WeekStartDate <= lastDayOfMonth
                && te.WeekEndDate >= firstDayOfMonth)
            .SumAsync(te => te.Hours);

        // Get leave balance for current year
        var leaveBalance = await _context.LeaveBalances
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == currentYear)
            .SumAsync(lb => lb.Total - lb.Used);

        // BonusBalance is managed by Spring Boot service, return 0 as placeholder
        // Frontend will fetch this from the bonus API separately
        return new EmployeeDashboardStatsDto(
            BonusBalance: 0,
            PendingTimesheets: pendingTimesheets,
            TotalHoursThisMonth: totalHoursThisMonth,
            LeaveBalance: leaveBalance
        );
    }

    /// <inheritdoc />
    public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync()
    {
        // Total employees count
        var totalEmployees = await _context.Employees.CountAsync();

        // Active employees count (status = "active", case-insensitive)
        var activeEmployees = await _context.Employees
            .Where(e => e.Status != null && e.Status.ToLower() == "active")
            .CountAsync();

        // Departments count
        var departments = await _context.Departments.CountAsync();

        // Total payroll - sum of Position.Salary for all employees with positions
        var totalPayroll = await _context.Employees
            .Where(e => e.Position != null)
            .SumAsync(e => e.Position!.Salary);

        // Pending timesheet approvals
        var pendingTimesheets = await _context.Requests
            .Where(r => r.Status == RequestStatus.Pending
                && r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category == "timesheet")
            .CountAsync();

        // Pending time-off approvals
        var pendingTimeOff = await _context.Requests
            .Where(r => r.Status == RequestStatus.Pending
                && r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category == "time-off")
            .CountAsync();

        return new AdminDashboardStatsDto(
            TotalEmployees: totalEmployees,
            ActiveEmployees: activeEmployees,
            Departments: departments,
            TotalPayroll: totalPayroll,
            PendingApprovals: new PendingApprovalsDto(
                Timesheets: pendingTimesheets,
                TimeOff: pendingTimeOff
            )
        );
    }
}
