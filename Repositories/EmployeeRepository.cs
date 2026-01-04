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

    public async Task<IReadOnlyList<Employee>> GetByManagerIdAsync(long managerId) =>
        await _db.Employees
            .Where(e => e.ManagerId == managerId)
            .OrderBy(e => e.Id)
            .AsNoTracking()
            .ToListAsync();

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

    public async Task<long> GetNextIdAsync()
    {
        var maxId = await _db.Employees.MaxAsync(e => (long?)e.Id) ?? 0;
        return maxId + 1;
    }

    public async Task<Employee?> GetByIdWithDetailsAsync(long id) =>
        await _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .Include(e => e.JobLevel)
            .Include(e => e.EmploymentType)
            .Include(e => e.TimeType)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<(IReadOnlyList<Employee> Employees, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        List<string>? statuses,
        List<string>? departments,
        List<string>? positions,
        List<string>? jobLevels,
        List<string>? employmentTypes,
        List<string>? timeTypes,
        int page,
        int pageSize)
    {
        IQueryable<Employee> query = _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .Include(e => e.JobLevel)
            .Include(e => e.EmploymentType)
            .Include(e => e.TimeType)
            .AsNoTracking();

        // Search term filter (OR logic: matches ID, FullName, or Email)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(e =>
                e.Id.ToString().Contains(searchTerm) ||
                e.FullName.ToLower().Contains(searchLower) ||
                e.Email.ToLower().Contains(searchLower));
        }

        // Status filter
        if (statuses != null && statuses.Count > 0)
        {
            // Map user-friendly status names to database values
            var dbStatuses = statuses.Select(s => s.ToUpper() switch
            {
                "PENDING" => "PENDING_ONBOARDING",
                "ACTIVE" => "ACTIVE",
                "INACTIVE" => "INACTIVE",
                _ => s.ToUpper()
            }).ToList();

            query = query.Where(e => e.Status != null && dbStatuses.Contains(e.Status));
        }

        // Department filter
        if (departments != null && departments.Count > 0)
        {
            query = query.Where(e => e.Department != null &&
                departments.Contains(e.Department.Name));
        }

        // Position filter
        if (positions != null && positions.Count > 0)
        {
            query = query.Where(e => e.Position != null &&
                positions.Contains(e.Position.Title));
        }

        // Job level filter
        if (jobLevels != null && jobLevels.Count > 0)
        {
            query = query.Where(e => e.JobLevel != null &&
                jobLevels.Contains(e.JobLevel.Name));
        }

        // Employment type filter
        if (employmentTypes != null && employmentTypes.Count > 0)
        {
            query = query.Where(e => e.EmploymentType != null &&
                employmentTypes.Contains(e.EmploymentType.Name));
        }

        // Time type filter
        if (timeTypes != null && timeTypes.Count > 0)
        {
            query = query.Where(e => e.TimeType != null &&
                timeTypes.Contains(e.TimeType.Name));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and sorting
        var employees = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (employees, totalCount);
    }

    public async Task<(int Total, int Onboarding, int Resigned, int Managers)> GetStatsAsync()
    {
        var total = await _db.Employees.CountAsync();

        var onboarding = await _db.Employees
            .CountAsync(e => e.Status == "PENDING_ONBOARDING");

        var resigned = await _db.Employees
            .CountAsync(e => e.Status == "INACTIVE");

        var managers = await _db.Employees
            .Include(e => e.Position)
            .Include(e => e.JobLevel)
            .CountAsync(e =>
                (e.Position != null && e.Position.Title.ToLower().Contains("manager")) ||
                (e.JobLevel != null && e.JobLevel.Name.ToLower() == "manager"));

        return (total, onboarding, resigned, managers);
    }
}
