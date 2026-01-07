using EmployeeApi.Dtos;
using EmployeeApi.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Services.Employee;

/// <summary>
/// Service for handling employee-related authorization and permission checks
/// </summary>
public class EmployeeAuthorizationService
{
    private readonly IEmployeeReadService _readService;
    private readonly AppDbContext _db;

    public EmployeeAuthorizationService(
        IEmployeeReadService readService,
        AppDbContext db)
    {
        _readService = readService;
        _db = db;
    }

    /// <summary>
    /// Checks if the current user can reassign supervisors for the specified employee.
    /// Only the HR (admin) who manages the employee can reassign supervisors.
    /// </summary>
    /// <param name="currentEmployeeId">The ID of the current authenticated user</param>
    /// <param name="targetEmployeeId">The ID of the employee whose supervisors are being reassigned</param>
    /// <returns>Authorization result with success status and error message if unauthorized</returns>
    public async Task<AuthorizationResult> CanReassignSupervisorsAsync(long currentEmployeeId, long targetEmployeeId)
    {
        // Get the employee being updated
        var employee = await _readService.GetOneAsync(targetEmployeeId);
        if (employee == null)
        {
            return AuthorizationResult.Failure("Employee not found");
        }

        // Check if current user is HR/admin
        var isHrOrAdmin = await _readService.IsHrOrAdminAsync(currentEmployeeId);
        if (!isHrOrAdmin)
        {
            return AuthorizationResult.Failure("Only HR personnel or admins can reassign supervisors");
        }

        // Check if current user is the HR manager of this employee
        // If employee has no HR assigned, any HR/admin can assign supervisors
        if (employee.HrId.HasValue && employee.HrId != currentEmployeeId)
        {
            return AuthorizationResult.Failure("Only the HR manager assigned to this employee can reassign supervisors");
        }

        return AuthorizationResult.Success();
    }

    /// <summary>
    /// Validates that the employee exists and returns the employee DTO
    /// </summary>
    public async Task<(bool IsValid, EmployeeDto? Employee, string? ErrorMessage)> ValidateEmployeeExistsAsync(long employeeId)
    {
        var employee = await _readService.GetOneAsync(employeeId);
        if (employee == null)
        {
            return (false, null, "Employee not found");
        }

        return (true, employee, null);
    }
}

/// <summary>
/// Result of an authorization check
/// </summary>
public class AuthorizationResult
{
    public bool IsAuthorized { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AuthorizationResult(bool isAuthorized, string? errorMessage = null)
    {
        IsAuthorized = isAuthorized;
        ErrorMessage = errorMessage;
    }

    public static AuthorizationResult Success() => new(true);
    public static AuthorizationResult Failure(string errorMessage) => new(false, errorMessage);
}

