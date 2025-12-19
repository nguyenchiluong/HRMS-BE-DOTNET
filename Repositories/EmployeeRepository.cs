using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;
using System.Linq.Expressions;

namespace EmployeeApi.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;
    public EmployeeRepository(AppDbContext db) => _db = db;

    public async Task<Employee?> GetByIdAsync(long id) =>
        await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Employee>> ListAsync(Expression<Func<Employee, bool>>? predicate = null)
    {
        IQueryable<Employee> query = _db.Employees.AsNoTracking();
        if (predicate != null)
            query = query.Where(predicate);
        return await query.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task AddAsync(Employee entity) => await _db.Employees.AddAsync(entity);

    public void Update(Employee entity) => _db.Employees.Update(entity);

    public void Remove(Employee entity) => _db.Employees.Remove(entity);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public Task<bool> ExistsByEmailAsync(string email) =>
        _db.Employees.AnyAsync(e => e.Email == email);

    public async Task<Employee?> GetByEmailAsync(string email) =>
        await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);
}
