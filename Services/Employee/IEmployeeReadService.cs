using EmployeeApi.Dtos;

namespace EmployeeApi.Services.Employee;

/// <summary>
/// Service for reading employee data
/// </summary>
public interface IEmployeeReadService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null);
    Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(long managerId);
    Task<EmployeeDto?> GetOneAsync(long id);

    /// <summary>
    /// Validates onboarding token and returns employee info for the onboarding form
    /// </summary>
    Task<EmployeeDto> GetByOnboardingTokenAsync(string token);

    /// <summary>
    /// Gets filtered and paginated employees for table view
    /// </summary>
    Task<EmployeePaginatedResponse<FilteredEmployeeDto>> GetFilteredAsync(
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
    Task<EmployeeStatsDto> GetStatsAsync();

    /// <summary>
    /// Gets all employees who can serve as managers
    /// </summary>
    Task<IEnumerable<ManagerOrHrDto>> GetManagersAsync(string? search = null);

    /// <summary>
    /// Gets all employees who are HR personnel
    /// </summary>
    Task<IEnumerable<ManagerOrHrDto>> GetHrPersonnelAsync(string? search = null);

    /// <summary>
    /// Checks if an employee is HR personnel or admin
    /// </summary>
    Task<bool> IsHrOrAdminAsync(long employeeId);

    /// <summary>
    /// Checks if an employee is an admin
    /// </summary>
    Task<bool> IsAdminAsync(long employeeId);
}

