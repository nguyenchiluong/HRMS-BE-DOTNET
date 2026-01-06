using EmployeeApi.Data;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<RequestRepository>? _logger;

    public RequestRepository(AppDbContext context, ILogger<RequestRepository>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Request>> GetRequestsAsync(
        long? employeeId = null,
        string? status = null,
        string? category = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20,
        long? managerId = null,
        bool filterByManagerReports = false,
        long? approverId = null,
        bool filterByApprover = false)
    {
        var query = _context.Requests
            .Include(r => r.Requester)
                .ThenInclude(e => e!.Department)
            .Include(r => r.Approver)
            .Include(r => r.RequestTypeLookup)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        // Manager filtering for approval request categories
        if (filterByManagerReports && managerId.HasValue)
        {
            var directReportIds = await GetDirectReportEmployeeIdsAsync(managerId.Value);
            _logger?.LogInformation(
                "GetRequestsAsync - Manager filtering: ManagerId={ManagerId}, DirectReportIds=[{DirectReportIds}]",
                managerId.Value,
                string.Join(", ", directReportIds));

            if (directReportIds.Count > 0)
            {
                query = query.Where(r => directReportIds.Contains(r.RequesterEmployeeId));
            }
            else
            {
                // Manager has no direct reports, return empty result
                query = query.Where(r => false);
            }
        }

        // Filter by approver for profile requests (admins only see requests assigned to them)
        if (filterByApprover && approverId.HasValue)
        {
            query = query.Where(r => r.ApproverEmployeeId == approverId.Value);
            _logger?.LogInformation(
                "GetRequestsAsync - Approver filtering applied: ApproverId={ApproverId}, Category={Category}",
                approverId.Value,
                category ?? "null");
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = EnumHelper.ParseRequestStatus(status);
            query = query.Where(r => r.Status == statusEnum);
        }

        // Filter by category
        if (!string.IsNullOrEmpty(category))
        {
            var normalizedCategory = category.ToLower().Trim();
            query = query.Where(r => r.RequestTypeLookup != null &&
                r.RequestTypeLookup.Category.ToLower() == normalizedCategory);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(r => r.EffectiveFrom >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(r => r.EffectiveTo <= dateTo.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetRequestsCountAsync(
        long? employeeId = null,
        string? status = null,
        string? category = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        long? managerId = null,
        bool filterByManagerReports = false,
        long? approverId = null,
        bool filterByApprover = false)
    {
        var query = _context.Requests.AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        // Manager filtering for approval request categories
        if (filterByManagerReports && managerId.HasValue)
        {
            var directReportIds = await GetDirectReportEmployeeIdsAsync(managerId.Value);
            query = query.Where(r => directReportIds.Contains(r.RequesterEmployeeId));
        }

        // Filter by approver for profile requests (admins only see requests assigned to them)
        if (filterByApprover && approverId.HasValue)
        {
            query = query.Where(r => r.ApproverEmployeeId == approverId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = EnumHelper.ParseRequestStatus(status);
            query = query.Where(r => r.Status == statusEnum);
        }

        // Filter by category
        if (!string.IsNullOrEmpty(category))
        {
            var normalizedCategory = category.ToLower().Trim();
            query = query.Where(r => r.RequestTypeLookup != null &&
                r.RequestTypeLookup.Category.ToLower() == normalizedCategory);
        }

        // Date filtering: For profile requests, filter by requestedAt; for others, filter by effectiveFrom/effectiveTo
        if (dateFrom.HasValue || dateTo.HasValue)
        {
            if (!string.IsNullOrEmpty(category) && category.ToLower() == "profile")
            {
                // For profile requests, filter by requestedAt
                if (dateFrom.HasValue)
                {
                    query = query.Where(r => r.RequestedAt >= dateFrom.Value);
                }
                if (dateTo.HasValue)
                {
                    query = query.Where(r => r.RequestedAt <= dateTo.Value);
                }
            }
            else
            {
                // For time-off and timesheet requests, filter by effectiveFrom/effectiveTo
                if (dateFrom.HasValue)
                {
                    query = query.Where(r => r.EffectiveFrom >= dateFrom.Value);
                }
                if (dateTo.HasValue)
                {
                    query = query.Where(r => r.EffectiveTo <= dateTo.Value);
                }
            }
        }

        return await query.CountAsync();
    }

    public async Task<List<long>> GetDirectReportEmployeeIdsAsync(long managerId)
    {
        // Get direct reports - employees who have this manager_id
        // This supports hierarchical reporting structure (employees -> managers -> senior managers -> CEO)
        var directReports = await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .Select(e => new { e.Id, e.Email, e.FullName })
            .ToListAsync();

        // Log for debugging
        _logger?.LogInformation(
            "GetDirectReportEmployeeIds - ManagerId: {ManagerId}, DirectReports: [{DirectReports}]",
            managerId,
            string.Join(", ", directReports.Select(dr => $"{dr.Id}({dr.Email})")));

        return directReports.Select(dr => dr.Id).ToList();
    }

    public async Task<bool> IsEmployeeUnderManagerAsync(long employeeId, long managerId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
        {
            return false;
        }

        // Check if manager is direct manager
        if (employee.ManagerId == managerId)
        {
            return true;
        }

        // Check if manager is in the reporting chain (hierarchical check)
        // This supports multi-level hierarchy: employee -> manager -> senior manager -> director -> CEO
        var currentManagerId = employee.ManagerId;
        var visited = new HashSet<long> { employeeId };

        while (currentManagerId.HasValue)
        {
            if (visited.Contains(currentManagerId.Value))
            {
                break; // Prevent infinite loop
            }
            visited.Add(currentManagerId.Value);

            if (currentManagerId.Value == managerId)
            {
                return true;
            }

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == currentManagerId.Value);

            if (manager == null)
            {
                break;
            }

            currentManagerId = manager.ManagerId;
        }

        return false;
    }

    public async Task<Request?> GetRequestByIdAsync(int id)
    {
        return await _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Approver)
            .Include(r => r.RequestTypeLookup)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Request> CreateRequestAsync(Request request)
    {
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<Request> UpdateRequestAsync(Request request)
    {
        request.UpdatedAt = DateTime.UtcNow;
        _context.Requests.Update(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<bool> DeleteRequestAsync(int id)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null)
        {
            return false;
        }

        _context.Requests.Remove(request);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<string, int>> GetRequestsSummaryByStatusAsync(
        long? employeeId = null,
        string? month = null,
        string? requestType = null)
    {
        var query = _context.Requests.AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(month))
        {
            var date = DateTime.Parse(month + "-01");
            var startOfMonth = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999, DateTimeKind.Utc);
            query = query.Where(r => r.CreatedAt >= startOfMonth && r.CreatedAt <= endOfMonth);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            // Normalize the request type string for comparison
            var normalizedType = requestType.ToUpper().Replace("-", "_");
            query = query.Where(r => r.RequestTypeLookup != null &&
                (r.RequestTypeLookup.Code == normalizedType || r.RequestTypeLookup.Code == requestType));
        }

        var summary = await query
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return summary.ToDictionary(s => s.Status.ToString().ToLower(), s => s.Count);
    }

    public async Task<Dictionary<string, int>> GetRequestsSummaryByTypeAsync(
        long? employeeId = null,
        string? month = null,
        string? requestType = null)
    {
        var query = _context.Requests.AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(month))
        {
            var date = DateTime.Parse(month + "-01");
            var startOfMonth = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999, DateTimeKind.Utc);
            query = query.Where(r => r.CreatedAt >= startOfMonth && r.CreatedAt <= endOfMonth);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            // Normalize the request type string for comparison
            var normalizedType = requestType.ToUpper().Replace("-", "_");
            query = query.Where(r => r.RequestTypeLookup != null &&
                (r.RequestTypeLookup.Code == normalizedType || r.RequestTypeLookup.Code == requestType));
        }

        var summary = await query
            .GroupBy(r => r.RequestTypeLookup != null ? r.RequestTypeLookup.Code : "UNKNOWN")
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return summary.ToDictionary(s => s.Type, s => s.Count);
    }
}
