using System.Security.Claims;

namespace EmployeeApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the employee ID from JWT claims if available directly.
    /// Note: If empId is not in the token, use IUserContextService.GetEmployeeIdFromClaimsAsync() 
    /// in your controller for async database lookup.
    /// </summary>
    public static int? TryGetEmployeeId(this ClaimsPrincipal principal)
    {
        var employeeIdClaim = principal.FindFirst("empId")?.Value 
                           ?? principal.FindFirst("employee_id")?.Value;

        if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var employeeId))
        {
            return employeeId;
        }

        return null;
    }

    /// <summary>
    /// Gets the user's role from JWT claims.
    /// The Java service sends roles as an array in the 'roles' claim.
    /// </summary>
    public static string GetRole(this ClaimsPrincipal principal)
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
            // Return first role, prioritizing ADMIN > MANAGER > USER
            if (roles.Any(r => r.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)))
                return "Admin";
            if (roles.Any(r => r.Equals("MANAGER", StringComparison.OrdinalIgnoreCase)))
                return "Manager";
            if (roles.Any(r => r.Equals("USER", StringComparison.OrdinalIgnoreCase)))
                return "Employee";
                
            return roles.First(); // Return first role if no match
        }

        return "Employee"; // Default role
    }

    /// <summary>
    /// Gets the user's email from JWT claims.
    /// The Java service uses 'mail' and 'sub' claims for email.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("mail")?.Value 
            ?? principal.FindFirst("email")?.Value 
            ?? principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("sub")?.Value; // sub is also email in your Java service
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
