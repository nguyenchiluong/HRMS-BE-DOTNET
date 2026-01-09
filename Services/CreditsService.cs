using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services;

/// <summary>
/// Request payload for creating a credits account
/// </summary>
public class CreateCreditsAccountRequest
{
    public long EmpId { get; set; }
    public int BonusPoint { get; set; } = 0;
}

/// <summary>
/// Response from credits service
/// </summary>
public class CreditsAccountResponse
{
    public long EmpId { get; set; }
    public int BonusPoint { get; set; }
}

/// <summary>
/// Service for interacting with the credits service
/// </summary>
public interface ICreditsService
{
    /// <summary>
    /// Creates a credits account for a new employee
    /// </summary>
    /// <param name="empId">Employee ID</param>
    /// <param name="bonusPoint">Initial bonus points (optional, defaults to 0)</param>
    /// <returns>Credits account response or null if service is unavailable</returns>
    Task<CreditsAccountResponse?> CreateAccountAsync(long empId, int bonusPoint = 0);
}

public class CreditsService : ICreditsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreditsService> _logger;

    public CreditsService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CreditsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CreditsAccountResponse?> CreateAccountAsync(long empId, int bonusPoint = 0)
    {
        try
        {
            var creditsBaseUrl = _configuration["AuthService:BaseUrl"] ?? "http://localhost:8080";
            var createAccountEndpoint = $"{creditsBaseUrl}/api/credits/accounts";

            var request = new CreateCreditsAccountRequest
            {
                EmpId = empId,
                BonusPoint = bonusPoint
            };

            _logger.LogInformation(
                "Creating credits account for employee {EmpId} with bonus points {BonusPoint}",
                empId, bonusPoint);

            var response = await _httpClient.PostAsJsonAsync(createAccountEndpoint, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreditsAccountResponse>();
                _logger.LogInformation(
                    "Successfully created credits account for employee {EmpId}",
                    empId);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Failed to create credits account for employee {EmpId}. Status: {Status}, Response: {Response}",
                empId, response.StatusCode, errorContent);

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error while creating credits account for employee {EmpId}",
                empId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while creating credits account for employee {EmpId}",
                empId);
            return null;
        }
    }
}
