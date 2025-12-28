namespace EmployeeApi.Services;

/// <summary>
/// Response from the auth service after successful registration
/// </summary>
public record AuthRegistrationResult(bool Success, string? ErrorMessage = null);

/// <summary>
/// Service interface for interacting with the external auth service
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account in the auth service
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">Generated password</param>
    /// <param name="role">User role (e.g., "USER", "ADMIN")</param>
    /// <param name="empId">Employee ID to link the account</param>
    /// <returns>Registration result indicating success or failure</returns>
    Task<AuthRegistrationResult> RegisterAccountAsync(string email, string password, string role, long empId);

    /// <summary>
    /// Generates a secure random password for new accounts
    /// </summary>
    /// <param name="length">Password length (default: 12)</param>
    /// <returns>Generated password</returns>
    string GenerateSecurePassword(int length = 12);
}

