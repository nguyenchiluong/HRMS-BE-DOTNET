using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<Employee?> GetByEmailAsync(string email);
    Task<long> GetNextIdAsync();
    Task<Employee?> GetByIdWithDetailsAsync(long id);

    /// <summary>
    /// Gets filtered and paginated employees with related data (Position, Department)
    /// </summary>
    Task<(IReadOnlyList<Employee> Employees, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        List<string>? statuses,
        List<string>? departments,
        List<string>? positions,
        List<string>? jobLevels,
        List<string>? employmentTypes,
        List<string>? timeTypes,
        int page,
        int pageSize);

    /// <summary>
    /// Gets employee statistics
    /// </summary>
    Task<(int Total, int Onboarding, int Resigned, int Managers)> GetStatsAsync();

    /// <summary>
    /// Lists employees reporting to the specified manager
    /// </summary>
    Task<IReadOnlyList<Employee>> GetByManagerIdAsync(long managerId);
}
