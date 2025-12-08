using System.Security.Claims;

namespace EmployeeApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the employee ID from JWT claims.
    /// Expects a claim named "employee_id" or "sub" in the token.
    /// </summary>
    public static int GetEmployeeId(this ClaimsPrincipal principal)
    {
        var employeeIdClaim = principal.FindFirst("employee_id")?.Value 
                           ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(employeeIdClaim))
        {
            throw new UnauthorizedAccessException("Employee ID not found in token");
        }

        if (!int.TryParse(employeeIdClaim, out var employeeId))
        {
            throw new UnauthorizedAccessException("Invalid employee ID in token");
        }

        return employeeId;
    }

    /// <summary>
    /// Gets the user's role from JWT claims.
    /// Expects a claim named "role" in the token.
    /// </summary>
    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("role")?.Value 
            ?? principal.FindFirst(ClaimTypes.Role)?.Value 
            ?? "Employee";
    }

    /// <summary>
    /// Gets the user's email from JWT claims.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("email")?.Value 
            ?? principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the user's name from JWT claims.
    /// </summary>
    public static string? GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("name")?.Value 
            ?? principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        var userRole = principal.GetRole();
        return userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user is a manager or admin.
    /// </summary>
    public static bool IsManagerOrAdmin(this ClaimsPrincipal principal)
    {
        var role = principal.GetRole();
        return role.Equals("Manager", StringComparison.OrdinalIgnoreCase) 
            || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }
}
