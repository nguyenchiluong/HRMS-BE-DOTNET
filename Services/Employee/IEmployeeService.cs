using EmployeeApi.Dtos;

namespace EmployeeApi.Services.Employee;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null);
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
}

