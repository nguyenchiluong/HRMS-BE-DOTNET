using EmployeeApi.Dtos;

namespace EmployeeApi.Services.Employee;

/// <summary>
/// Service for writing/updating employee data
/// </summary>
public interface IEmployeeWriteService
{
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
    /// Saves onboarding progress without completing it
    /// </summary>
    Task<EmployeeDto> SaveOnboardingProgressAsync(string token, OnboardDto input);

    /// <summary>
    /// Updates the current employee's profile information
    /// </summary>
    Task<EmployeeDto> UpdateProfileAsync(long employeeId, UpdateProfileDto input);

    /// <summary>
    /// Reassigns supervisors (manager and HR) for an employee
    /// </summary>
    Task ReassignSupervisorsAsync(long employeeId, ReassignSupervisorsDto input);
}

