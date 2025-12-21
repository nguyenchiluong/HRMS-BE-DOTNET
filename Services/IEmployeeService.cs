using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

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
}
