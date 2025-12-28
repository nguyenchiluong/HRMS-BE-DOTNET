using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services;

/// <summary>
/// Request payload for auth service registration
/// </summary>
public class AuthRegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Role { get; set; } = default!;
    public long EmpId { get; set; }
}

/// <summary>
/// Service for interacting with the external auth service
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    // Characters for password generation
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*";

    public AuthService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthRegistrationResult> RegisterAccountAsync(string email, string password, string role, long empId)
    {
        try
        {
            var authBaseUrl = _configuration["AuthService:BaseUrl"] ?? "http://localhost:8080";
            var registerEndpoint = $"{authBaseUrl}/auth/register";

            var request = new AuthRegisterRequest
            {
                Email = email,
                Password = password,
                Role = role,
                EmpId = empId
            };

            _logger.LogInformation(
                "Registering account for employee {EmpId} with email {Email}",
                empId, email);

            var response = await _httpClient.PostAsJsonAsync(registerEndpoint, request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully registered account for employee {EmpId}",
                    empId);
                return new AuthRegistrationResult(true);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Failed to register account for employee {EmpId}. Status: {Status}, Response: {Response}",
                empId, response.StatusCode, errorContent);

            return new AuthRegistrationResult(false, $"Auth service returned {response.StatusCode}: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error while registering account for employee {EmpId}",
                empId);
            return new AuthRegistrationResult(false, $"Failed to connect to auth service: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while registering account for employee {EmpId}",
                empId);
            return new AuthRegistrationResult(false, $"Unexpected error: {ex.Message}");
        }
    }

    public string GenerateSecurePassword(int length = 12)
    {
        if (length < 8)
            length = 8; // Minimum length for security

        var allChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;
        var password = new StringBuilder();

        // Ensure at least one character from each category
        password.Append(GetRandomChar(LowercaseChars));
        password.Append(GetRandomChar(UppercaseChars));
        password.Append(GetRandomChar(DigitChars));
        password.Append(GetRandomChar(SpecialChars));

        // Fill the rest with random characters
        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        // Shuffle the password characters
        return ShuffleString(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        return chars[RandomNumberGenerator.GetInt32(chars.Length)];
    }

    private static string ShuffleString(string input)
    {
        var array = input.ToCharArray();
        var n = array.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return new string(array);
    }
}

