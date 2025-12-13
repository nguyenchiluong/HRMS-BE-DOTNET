using System.Security.Claims;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services;

public interface IUserContextService
{
    Task<int> GetEmployeeIdFromClaimsAsync(ClaimsPrincipal principal);
    string GetEmailFromClaims(ClaimsPrincipal principal);
    string GetRoleFromClaims(ClaimsPrincipal principal);
}

public class UserContextService : IUserContextService
{
    private readonly IEmployeeRepository _employeeRepository;

    public UserContextService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<int> GetEmployeeIdFromClaimsAsync(ClaimsPrincipal principal)
    {
        // First, try to get employee_id directly from token if it exists
        var employeeIdClaim = principal.FindFirst("empId")?.Value 
                           ?? principal.FindFirst("employee_id")?.Value;

        if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var employeeId))
        {
            return employeeId;
        }

        // If no employee_id in token, map from email
        var email = GetEmailFromClaims(principal);
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("Unable to identify user from token");
        }

        // Look up employee by email
        var employee = await _employeeRepository.GetByEmailAsync(email);

        if (employee == null)
        {
            throw new UnauthorizedAccessException($"No employee found with email: {email}");
        }

        return employee.Id;
    }

    public string GetEmailFromClaims(ClaimsPrincipal principal)
    {
        return principal.FindFirst("mail")?.Value 
            ?? principal.FindFirst("email")?.Value 
            ?? principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Email not found in token");
    }

    public string GetRoleFromClaims(ClaimsPrincipal principal)
    {
        // Try standard role claim first
        var roleClaim = principal.FindFirst("role")?.Value 
                     ?? principal.FindFirst(ClaimTypes.Role)?.Value;
        
        if (!string.IsNullOrEmpty(roleClaim))
        {
            return roleClaim;
        }

        // Check for roles array (the Java service uses 'roles')
        var roles = principal.FindAll("roles").Select(c => c.Value).ToList();
        
        if (roles.Any())
        {
            // Map Java roles to application roles
            if (roles.Any(r => r.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)))
                return "Admin";
            if (roles.Any(r => r.Equals("MANAGER", StringComparison.OrdinalIgnoreCase)))
                return "Manager";
            if (roles.Any(r => r.Equals("USER", StringComparison.OrdinalIgnoreCase)))
                return "Employee";
                
            return roles.First();
        }

        return "Employee"; // Default role
    }
}
