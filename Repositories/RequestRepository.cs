using EmployeeApi.Data;
using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly AppDbContext _context;

    public RequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Request>> GetRequestsAsync(
        int? employeeId = null,
        string? status = null,
        string? requestType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20)
    {
        var query = _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Approver)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            query = query.Where(r => r.RequestType == requestType);
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
        int? employeeId = null,
        string? status = null,
        string? requestType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.Requests.AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.RequesterEmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            query = query.Where(r => r.RequestType == requestType);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(r => r.EffectiveFrom >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(r => r.EffectiveTo <= dateTo.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Request?> GetRequestByIdAsync(int id)
    {
        return await _context.Requests
            .Include(r => r.Requester)
            .Include(r => r.Approver)
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
        int? employeeId = null,
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
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            query = query.Where(r => r.CreatedAt >= startOfMonth && r.CreatedAt <= endOfMonth);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            query = query.Where(r => r.RequestType == requestType);
        }

        var summary = await query
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return summary.ToDictionary(s => s.Status.ToLower(), s => s.Count);
    }

    public async Task<Dictionary<string, int>> GetRequestsSummaryByTypeAsync(
        int? employeeId = null,
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
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            query = query.Where(r => r.CreatedAt >= startOfMonth && r.CreatedAt <= endOfMonth);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            query = query.Where(r => r.RequestType == requestType);
        }

        var summary = await query
            .GroupBy(r => r.RequestType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return summary.ToDictionary(s => s.Type, s => s.Count);
    }
}
