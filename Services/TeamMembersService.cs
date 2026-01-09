using EmployeeApi.Data;
using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Services;

public class TeamMembersService : ITeamMembersService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TeamMembersService> _logger;

    public TeamMembersService(
        AppDbContext context,
        ILogger<TeamMembersService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TeamMembersSummaryDto> GetTeamMembersSummaryAsync(long managerId)
    {
        // Get all team members (no filters for summary)
        var teamMemberIds = await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .Select(e => e.Id)
            .ToListAsync();

        if (teamMemberIds.Count == 0)
        {
            return new TeamMembersSummaryDto
            {
                ActiveMembers = 0,
                ClockedInCount = 0,
                TotalPendingTimesheets = 0,
                TotalPendingTimeOff = 0
            };
        }

        // Count active members
        var activeMembers = await _context.Employees
            .Where(e => e.ManagerId == managerId && e.Status == "ACTIVE")
            .CountAsync();

        // Count clocked-in members (today's attendance with CheckInTime but no CheckOutTime)
        var today = DateTime.UtcNow.Date;
        var clockedInCount = await _context.AttendanceRecords
            .Where(a => teamMemberIds.Contains(a.EmployeeId) 
                && a.Date == today 
                && a.CheckOutTime == null)
            .Select(a => a.EmployeeId)
            .Distinct()
            .CountAsync();

        // Count pending timesheets
        var pendingTimesheets = await _context.Requests
            .Where(r => r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category.ToLower() == "timesheet"
                && r.Status == RequestStatus.Pending
                && teamMemberIds.Contains(r.RequesterEmployeeId))
            .CountAsync();

        // Count pending time-off requests
        var pendingTimeOff = await _context.Requests
            .Where(r => r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category.ToLower() == "time-off"
                && r.Status == RequestStatus.Pending
                && teamMemberIds.Contains(r.RequesterEmployeeId))
            .CountAsync();

        return new TeamMembersSummaryDto
        {
            ActiveMembers = activeMembers,
            ClockedInCount = clockedInCount,
            TotalPendingTimesheets = pendingTimesheets,
            TotalPendingTimeOff = pendingTimeOff
        };
    }

    public async Task<TeamMembersResponseDto> GetTeamMembersAsync(
        long managerId,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? department = null,
        string? status = null,
        string? position = null)
    {
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Build base query for team members
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.JobLevel)
            .Include(e => e.EmploymentType)
            .Include(e => e.TimeType)
            .Where(e => e.ManagerId == managerId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(searchLower) ||
                e.Email.ToLower().Contains(searchLower) ||
                (e.Position != null && e.Position.Title.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(e => e.Department != null && e.Department.Name == department);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            // Map user-friendly status to database values
            var dbStatus = status.ToUpper() switch
            {
                "PENDING" => "PENDING_ONBOARDING",
                "ACTIVE" => "ACTIVE",
                "INACTIVE" => "INACTIVE",
                _ => status.ToUpper()
            };
            query = query.Where(e => e.Status == dbStatus);
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(e => e.Position != null && e.Position.Title == position);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var employees = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get employee IDs for batch queries
        var employeeIds = employees.Select(e => e.Id).ToList();

        // Get today's attendance records for all employees in batch
        var today = DateTime.UtcNow.Date;
        var attendanceRecords = await _context.AttendanceRecords
            .Where(a => employeeIds.Contains(a.EmployeeId) && a.Date == today)
            .ToListAsync();

        var attendanceDict = attendanceRecords.ToDictionary(a => a.EmployeeId);

        // Get pending timesheet counts for all employees in batch
        var pendingTimesheetCounts = await _context.Requests
            .Where(r => r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category.ToLower() == "timesheet"
                && r.Status == RequestStatus.Pending
                && employeeIds.Contains(r.RequesterEmployeeId))
            .GroupBy(r => r.RequesterEmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToListAsync();

        var timesheetCountDict = pendingTimesheetCounts.ToDictionary(x => x.EmployeeId, x => x.Count);

        // Get pending time-off counts for all employees in batch
        var pendingTimeOffCounts = await _context.Requests
            .Where(r => r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category.ToLower() == "time-off"
                && r.Status == RequestStatus.Pending
                && employeeIds.Contains(r.RequesterEmployeeId))
            .GroupBy(r => r.RequesterEmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToListAsync();

        var timeOffCountDict = pendingTimeOffCounts.ToDictionary(x => x.EmployeeId, x => x.Count);

        // Get last timesheet status for all employees in batch
        var lastTimesheetStatuses = await _context.Requests
            .Where(r => r.RequestTypeLookup != null
                && r.RequestTypeLookup.Category.ToLower() == "timesheet"
                && employeeIds.Contains(r.RequesterEmployeeId))
            .OrderByDescending(r => r.CreatedAt)
            .GroupBy(r => r.RequesterEmployeeId)
            .Select(g => new { EmployeeId = g.Key, Status = g.First().Status })
            .ToListAsync();

        var lastTimesheetStatusDict = lastTimesheetStatuses.ToDictionary(
            x => x.EmployeeId,
            x => x.Status.ToString().ToUpper());

        // Map to DTOs
        var teamMembers = employees.Select(employee =>
        {
            var attendanceRecord = attendanceDict.GetValueOrDefault(employee.Id);
            var attendanceStatus = "clocked-out";
            string? clockInTime = null;
            int? currentWorkingMinutes = null;

            if (attendanceRecord != null && attendanceRecord.CheckOutTime == null)
            {
                attendanceStatus = "clocked-in";
                clockInTime = attendanceRecord.CheckInTime.ToString("O");
                var now = DateTime.UtcNow;
                currentWorkingMinutes = (int)Math.Round((now - attendanceRecord.CheckInTime).TotalMinutes);
            }

            var pendingTimesheetCount = timesheetCountDict.GetValueOrDefault(employee.Id, 0);
            var pendingTimeOffCount = timeOffCountDict.GetValueOrDefault(employee.Id, 0);
            var lastTimesheetStatus = lastTimesheetStatusDict.GetValueOrDefault(employee.Id);

            // Map status to user-friendly format
            var userFriendlyStatus = employee.Status?.ToUpper() switch
            {
                "PENDING_ONBOARDING" => "Pending",
                "ACTIVE" => "Active",
                "INACTIVE" => "Inactive",
                _ => employee.Status
            };

            return new TeamMemberDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                WorkEmail = employee.Email,
                Position = employee.Position?.Title,
                Department = employee.Department?.Name,
                JobLevel = employee.JobLevel?.Name,
                Status = userFriendlyStatus,
                EmploymentType = employee.EmploymentType?.Name,
                TimeType = employee.TimeType?.Name,
                Avatar = employee.Avatar,
                Phone = employee.Phone,
                StartDate = employee.StartDate,
                AttendanceStatus = attendanceStatus,
                ClockInTime = clockInTime,
                CurrentWorkingMinutes = currentWorkingMinutes,
                PendingTimesheetCount = pendingTimesheetCount,
                PendingTimeOffCount = pendingTimeOffCount,
                LastTimesheetStatus = lastTimesheetStatus
            };
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new TeamMembersResponseDto
        {
            TeamMembers = teamMembers,
            TotalRecords = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
}
