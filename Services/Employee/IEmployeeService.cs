using EmployeeApi.Dtos;

namespace EmployeeApi.Services.Employee;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null);
    Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(long managerId);
    Task<EmployeeDto?> GetOneAsync(long id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto input);
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// Admin creates initial profile for a new hire
    /// </summary>
    Task<EmployeeDto> CreateInitialProfileAsync(InitialProfileDto input);

    /// <summary>
    /// New hire completes their onboarding with personal details
    /// </summary>
    Task<EmployeeDto> CompleteOnboardingAsync(long employeeId, OnboardDto input);

    /// <summary>
    /// Validates onboarding token and returns employee info for the onboarding form
    /// </summary>
    Task<EmployeeDto> GetByOnboardingTokenAsync(string token);

    /// <summary>
    /// Saves onboarding progress without completing it
    /// </summary>
    Task<EmployeeDto> SaveOnboardingProgressAsync(string token, OnboardDto input);

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
    /// Updates the current employee's profile information
    /// </summary>
    Task<EmployeeDto> UpdateProfileAsync(long employeeId, UpdateProfileDto input);

    /// <summary>
    /// Gets all employees who can serve as managers
    /// </summary>
    Task<IEnumerable<ManagerOrHrDto>> GetManagersAsync(string? search = null);

    /// <summary>
    /// Gets all employees who are HR personnel
    /// </summary>
    Task<IEnumerable<ManagerOrHrDto>> GetHrPersonnelAsync(string? search = null);
}

