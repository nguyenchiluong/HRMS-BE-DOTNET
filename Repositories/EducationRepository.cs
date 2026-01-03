using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public class EducationRepository : IEducationRepository
{
    private readonly AppDbContext _context;

    public EducationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Education?> GetByIdAsync(long id)
    {
        return await _context.Educations.FindAsync(id);
    }

    public async Task<IReadOnlyList<Education>> ListAsync(Expression<Func<Education, bool>>? predicate = null)
    {
        var query = _context.Educations.AsQueryable();
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.ToListAsync();
    }

    public async Task AddAsync(Education entity)
    {
        await _context.Educations.AddAsync(entity);
    }

    public void Update(Education entity)
    {
        _context.Educations.Update(entity);
    }

    public void Remove(Education entity)
    {
        _context.Educations.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Education>> GetByEmployeeIdAsync(long employeeId)
    {
        return await _context.Educations
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.Id)
            .ToListAsync();
    }

    public async Task<Education?> GetByIdAndEmployeeIdAsync(long id, long employeeId)
    {
        var query = _context.Educations.Where(e => e.Id == id);
        
        // If employeeId is 0, it means admin access - skip employee filter
        if (employeeId > 0)
        {
            query = query.Where(e => e.EmployeeId == employeeId);
        }
        
        return await query.FirstOrDefaultAsync();
    }
}
