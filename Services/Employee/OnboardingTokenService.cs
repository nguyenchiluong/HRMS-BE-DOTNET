using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services.Employee;

/// <summary>
/// Result of token validation
/// </summary>
public record TokenValidationResult(long EmployeeId, string? ErrorMessage)
{
    public bool IsValid => ErrorMessage == null;
}

/// <summary>
/// Service interface for onboarding token operations
/// </summary>
public interface IOnboardingTokenService
{
    /// <summary>
    /// Generates a secure token for the onboarding form link
    /// </summary>
    string GenerateToken(long employeeId);

    /// <summary>
    /// Validates the onboarding token and extracts the employee ID
    /// </summary>
    TokenValidationResult ValidateToken(string token);

    /// <summary>
    /// Generates the full onboarding link for an employee
    /// </summary>
    string GenerateOnboardingLink(long employeeId);
}

/// <summary>
/// Service for generating and validating onboarding tokens
/// </summary>
public class OnboardingTokenService : IOnboardingTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OnboardingTokenService> _logger;

    public OnboardingTokenService(
        IConfiguration configuration,
        ILogger<OnboardingTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(long employeeId)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "default-secret-key";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = $"{employeeId}:{timestamp}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(hash);

        // Combine payload and signature, URL-safe encoding
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}:{signature}"));
        return Uri.EscapeDataString(token);
    }

    public TokenValidationResult ValidateToken(string token)
    {
        try
        {
            // URL decode and base64 decode the token
            var decodedToken = Uri.UnescapeDataString(token);
            var tokenBytes = Convert.FromBase64String(decodedToken);
            var tokenString = Encoding.UTF8.GetString(tokenBytes);

            // Parse: {employeeId}:{timestamp}:{signature}
            var parts = tokenString.Split(':');
            if (parts.Length != 3)
                return new TokenValidationResult(0, "Invalid token format");

            if (!long.TryParse(parts[0], out var employeeId))
                return new TokenValidationResult(0, "Invalid employee ID in token");

            if (!long.TryParse(parts[1], out var timestamp))
                return new TokenValidationResult(0, "Invalid timestamp in token");

            var providedSignature = parts[2];

            // Verify signature
            var secretKey = _configuration["Jwt:SecretKey"] ?? "default-secret-key";
            var payload = $"{employeeId}:{timestamp}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);

            if (providedSignature != expectedSignature)
                return new TokenValidationResult(0, "Invalid token signature");

            // Check expiration (default 72 hours)
            var expirationHours = _configuration.GetValue<int>("Application:OnboardingTokenExpirationHours", 72);
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var expirationTime = tokenTime.AddHours(expirationHours);

            if (DateTimeOffset.UtcNow > expirationTime)
                return new TokenValidationResult(0, "Token has expired");

            return new TokenValidationResult(employeeId, null);
        }
        catch (FormatException)
        {
            return new TokenValidationResult(0, "Invalid token encoding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating onboarding token");
            return new TokenValidationResult(0, "Token validation failed");
        }
    }

    public string GenerateOnboardingLink(long employeeId)
    {
        var secureToken = GenerateToken(employeeId);
        var onboardingBaseUrl = _configuration["Application:OnboardingBaseUrl"]
            ?? _configuration["Application:BaseUrl"]
            ?? "http://localhost:3000";

        return $"{onboardingBaseUrl}/onboarding?token={secureToken}&employeeId={employeeId}";
    }
}

