using EmployeeApi.Data;
using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Repositories;

public class RequestTypeRepository : IRequestTypeRepository
{
    private readonly AppDbContext _context;

    public RequestTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RequestTypeLookup>> GetAllRequestTypesAsync(bool activeOnly = true)
    {
        var query = _context.RequestTypeLookups.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(rt => rt.IsActive);
        }

        return await query
            .OrderBy(rt => rt.Category)
            .ThenBy(rt => rt.Name)
            .ToListAsync();
    }

    public async Task<RequestTypeLookup?> GetRequestTypeByCodeAsync(string code)
    {
        return await _context.RequestTypeLookups
            .FirstOrDefaultAsync(rt => rt.Code == code);
    }

    public async Task<RequestTypeLookup?> GetRequestTypeByIdAsync(long id)
    {
        return await _context.RequestTypeLookups
            .FirstOrDefaultAsync(rt => rt.Id == id);
    }
}

