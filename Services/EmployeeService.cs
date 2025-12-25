using EmployeeApi.Dtos;
using EmployeeApi.Helpers;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _db;
    private readonly IMessageProducerService _messageProducer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmployeeService> _logger;

    private const string SEND_EMAIL_QUEUE = "sendEmail";

    public EmployeeService(
        IEmployeeRepository repo,
        AppDbContext db,
        IMessageProducerService messageProducer,
        IConfiguration configuration,
        ILogger<EmployeeService> logger)
    {
        _repo = repo;
        _db = db;
        _messageProducer = messageProducer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null)
    {
        var list = await _repo.ListAsync(string.IsNullOrWhiteSpace(search) ? null :
            e => e.FullName.Contains(search!) || e.Email.Contains(search!));
        return list.Select(ToDto);
    }

    public async Task<EmployeeDto?> GetOneAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : ToDto(e);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required");
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required");

        if (await _repo.ExistsByEmailAsync(input.Email.Trim()))
            throw new InvalidOperationException("Employee already exists");

        var entity = new Employee
        {
            Id = await _repo.GetNextIdAsync(),
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            Phone = input.Phone,
            StartDate = input.StartDate,
            PositionId = input.PositionId,
            DepartmentId = input.DepartmentId,
            ManagerId = input.ManagerId,
            Status = input.Status ?? EmployeeStatus.PendingOnboarding.ToApiString(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<EmployeeDto> CreateInitialProfileAsync(InitialProfileDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required");
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(input.JobLevel))
            throw new ArgumentException("JobLevel is required");
        if (string.IsNullOrWhiteSpace(input.EmployeeType))
            throw new ArgumentException("EmployeeType is required");
        if (string.IsNullOrWhiteSpace(input.TimeType))
            throw new ArgumentException("TimeType is required");

        if (await _repo.ExistsByEmailAsync(input.Email.Trim()))
            throw new InvalidOperationException("Employee with this email already exists");

        // Validate foreign keys
        var department = await _db.Departments.FindAsync(input.DepartmentId);
        if (department is null)
            throw new ArgumentException($"Department with ID {input.DepartmentId} does not exist");

        var position = await _db.Positions.FindAsync(input.PositionId);
        if (position is null)
            throw new ArgumentException($"Position with ID {input.PositionId} does not exist");

        if (input.ManagerId.HasValue)
        {
            var manager = await _repo.GetByIdAsync(input.ManagerId.Value);
            if (manager is null)
                throw new ArgumentException($"Manager with ID {input.ManagerId} does not exist");
        }

        var entity = new Employee
        {
            Id = await _repo.GetNextIdAsync(),
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            PositionId = input.PositionId,
            JobLevel = input.JobLevel.Trim(),
            DepartmentId = input.DepartmentId,
            EmployeeType = input.EmployeeType.Trim(),
            TimeType = input.TimeType.Trim(),
            StartDate = input.StartDate,
            ManagerId = input.ManagerId,
            Status = EmployeeStatus.PendingOnboarding.ToApiString(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        // Publish event to send onboarding email to the new employee
        await PublishOnboardingEmailEvent(entity);

        return ToDto(entity);
    }

    /// <summary>
    /// Publishes an event to send onboarding email to the new employee
    /// </summary>
    private async Task PublishOnboardingEmailEvent(Employee employee)
    {
        try
        {
            var personalEmail = employee.PersonalEmail ?? employee.Email;
            var secureToken = GenerateSecureOnboardingToken(employee.Id);
            var onboardingBaseUrl = _configuration["Application:OnboardingBaseUrl"]
                ?? _configuration["Application:BaseUrl"]
                ?? "http://localhost:3000";
            var onboardingLink = $"{onboardingBaseUrl}/onboarding?token={secureToken}&employeeId={employee.Id}";

            var emailEvent = new SendEmailEvent
            {
                EmailToSend = personalEmail,
                Subject = $"Welcome to the Team, {employee.FullName}! Complete Your Onboarding",
                HtmlContent = GenerateOnboardingEmailHtml(employee, onboardingLink)
            };

            await _messageProducer.PublishMessage(emailEvent, SEND_EMAIL_QUEUE);
            _logger.LogInformation(
                "Published onboarding email event for employee {EmployeeId} to {PersonalEmail}",
                employee.Id, personalEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish onboarding email event for employee {EmployeeId}",
                employee.Id);
            // Don't throw - email sending failure shouldn't fail the profile creation
        }
    }

    /// <summary>
    /// Generates a secure token for the onboarding form link
    /// </summary>
    private string GenerateSecureOnboardingToken(long employeeId)
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

    /// <summary>
    /// Generates the HTML content for the onboarding email
    /// </summary>
    private static string GenerateOnboardingEmailHtml(Employee employee, string onboardingLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;"">
        <h1 style=""color: white; margin: 0;"">Welcome to the Team!</h1>
    </div>
    
    <div style=""background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 16px;"">Dear <strong>{employee.FullName}</strong>,</p>
        
        <p style=""font-size: 16px;"">We are thrilled to welcome you to our organization! Your initial profile has been created, and we're excited to have you join us.</p>
        
        <p style=""font-size: 16px;"">To complete your onboarding process, please click the button below to fill in your personal details:</p>
        
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{onboardingLink}"" style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;"">Complete Onboarding</a>
        </div>
        
        <p style=""font-size: 14px; color: #666;"">If the button doesn't work, you can copy and paste the following link into your browser:</p>
        <p style=""font-size: 12px; color: #888; word-break: break-all;"">{onboardingLink}</p>
        
        <hr style=""border: none; border-top: 1px solid #ddd; margin: 30px 0;"">
        
        <p style=""font-size: 14px; color: #666;"">Your work email: <strong>{employee.Email}</strong></p>
        <p style=""font-size: 14px; color: #666;"">Start date: <strong>{employee.StartDate?.ToString("MMMM dd, yyyy") ?? "To be confirmed"}</strong></p>
        
        <p style=""font-size: 16px; margin-top: 30px;"">If you have any questions, please don't hesitate to reach out to your HR representative.</p>
        
        <p style=""font-size: 16px;"">Best regards,<br><strong>HR Team</strong></p>
    </div>
    
    <div style=""text-align: center; padding: 20px; font-size: 12px; color: #888;"">
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
    }

    public async Task<EmployeeDto> CompleteOnboardingAsync(long employeeId, OnboardDto input)
    {
        var employee = await _repo.GetByIdAsync(employeeId);
        if (employee is null)
            throw new KeyNotFoundException("Employee not found");

        if (employee.Status == EmployeeStatus.Active.ToApiString())
            throw new InvalidOperationException("Onboarding has already been completed");

        // Update personal details
        employee.FirstName = input.FirstName;
        employee.LastName = input.LastName;
        employee.PreferredName = input.PreferredName;
        employee.Sex = input.Sex;
        employee.DateOfBirth = input.DateOfBirth;
        employee.MaritalStatus = input.MaritalStatus;
        employee.Pronoun = input.Pronoun;
        employee.PersonalEmail = input.PersonalEmail;
        employee.Phone = input.Phone;
        employee.Phone2 = input.Phone2;

        // Update address
        employee.PermanentAddress = input.PermanentAddress;
        employee.CurrentAddress = input.CurrentAddress;

        // Update National ID
        if (input.NationalId != null)
        {
            employee.NationalIdCountry = input.NationalId.Country;
            employee.NationalIdNumber = input.NationalId.Number;
            employee.NationalIdIssuedDate = input.NationalId.IssuedDate;
            employee.NationalIdExpirationDate = input.NationalId.ExpirationDate;
            employee.NationalIdIssuedBy = input.NationalId.IssuedBy;
        }

        // Update Social Insurance & Tax
        employee.SocialInsuranceNumber = input.SocialInsuranceNumber;
        employee.TaxId = input.TaxId;

        // Mark onboarding as completed
        employee.Status = EmployeeStatus.Active.ToApiString();
        employee.UpdatedAt = DateTime.Now;

        _repo.Update(employee);

        // Add education records if provided
        if (input.Education != null && input.Education.Count > 0)
        {
            foreach (var edu in input.Education)
            {
                var education = new Education
                {
                    EmployeeId = employeeId,
                    Degree = edu.Degree,
                    FieldOfStudy = edu.FieldOfStudy,
                    Gpa = edu.Gpa,
                    Country = edu.Country
                };
                await _db.Educations.AddAsync(education);
            }
        }

        // Add bank account if provided
        if (input.BankAccount != null &&
            !string.IsNullOrWhiteSpace(input.BankAccount.BankName) &&
            !string.IsNullOrWhiteSpace(input.BankAccount.AccountNumber))
        {
            var bankAccount = new BankAccount
            {
                EmployeeId = employeeId,
                BankName = input.BankAccount.BankName.Trim(),
                AccountNumber = input.BankAccount.AccountNumber.Trim(),
                AccountName = input.BankAccount.AccountName?.Trim()
            };
            await _db.BankAccounts.AddAsync(bankAccount);
        }

        await _repo.SaveChangesAsync();
        return ToDto(employee);
    }

    public async Task<EmployeeDto> GetByOnboardingTokenAsync(string token)
    {
        var (employeeId, errorMessage) = ValidateOnboardingToken(token);
        if (errorMessage != null)
            throw new ArgumentException(errorMessage);

        var employee = await _repo.GetByIdWithDetailsAsync(employeeId);
        if (employee is null)
            throw new KeyNotFoundException("Employee not found");

        return ToDto(employee);
    }

    public async Task<EmployeeDto> SaveOnboardingProgressAsync(string token, OnboardDto input)
    {
        var (employeeId, errorMessage) = ValidateOnboardingToken(token);
        if (errorMessage != null)
            throw new ArgumentException(errorMessage);

        var employee = await _repo.GetByIdAsync(employeeId);
        if (employee is null)
            throw new KeyNotFoundException("Employee not found");

        if (employee.Status == EmployeeStatus.Active.ToApiString())
            throw new InvalidOperationException("Onboarding has already been completed");

        // Update personal details
        employee.FirstName = input.FirstName;
        employee.LastName = input.LastName;
        employee.PreferredName = input.PreferredName;
        employee.Sex = input.Sex;
        employee.DateOfBirth = input.DateOfBirth;
        employee.MaritalStatus = input.MaritalStatus;
        employee.Pronoun = input.Pronoun;
        employee.PersonalEmail = input.PersonalEmail;
        employee.Phone = input.Phone;
        employee.Phone2 = input.Phone2;

        // Update address
        employee.PermanentAddress = input.PermanentAddress;
        employee.CurrentAddress = input.CurrentAddress;

        // Update National ID
        if (input.NationalId != null)
        {
            employee.NationalIdCountry = input.NationalId.Country;
            employee.NationalIdNumber = input.NationalId.Number;
            employee.NationalIdIssuedDate = input.NationalId.IssuedDate;
            employee.NationalIdExpirationDate = input.NationalId.ExpirationDate;
            employee.NationalIdIssuedBy = input.NationalId.IssuedBy;
        }

        // Update Social Insurance & Tax
        employee.SocialInsuranceNumber = input.SocialInsuranceNumber;
        employee.TaxId = input.TaxId;

        // Keep status as PENDING_ONBOARDING (don't complete)
        employee.UpdatedAt = DateTime.Now;

        _repo.Update(employee);

        // Handle education records - remove existing and add new
        var existingEducations = _db.Educations.Where(e => e.EmployeeId == employeeId);
        _db.Educations.RemoveRange(existingEducations);

        if (input.Education != null && input.Education.Count > 0)
        {
            foreach (var edu in input.Education)
            {
                var education = new Education
                {
                    EmployeeId = employeeId,
                    Degree = edu.Degree,
                    FieldOfStudy = edu.FieldOfStudy,
                    Gpa = edu.Gpa,
                    Country = edu.Country
                };
                await _db.Educations.AddAsync(education);
            }
        }

        // Handle bank account - remove existing and add new
        var existingBankAccounts = _db.BankAccounts.Where(b => b.EmployeeId == employeeId);
        _db.BankAccounts.RemoveRange(existingBankAccounts);

        if (input.BankAccount != null &&
            !string.IsNullOrWhiteSpace(input.BankAccount.BankName) &&
            !string.IsNullOrWhiteSpace(input.BankAccount.AccountNumber))
        {
            var bankAccount = new BankAccount
            {
                EmployeeId = employeeId,
                BankName = input.BankAccount.BankName.Trim(),
                AccountNumber = input.BankAccount.AccountNumber.Trim(),
                AccountName = input.BankAccount.AccountName?.Trim()
            };
            await _db.BankAccounts.AddAsync(bankAccount);
        }

        await _repo.SaveChangesAsync();
        return ToDto(employee);
    }

    /// <summary>
    /// Validates the onboarding token and extracts the employee ID
    /// </summary>
    private (long employeeId, string? errorMessage) ValidateOnboardingToken(string token)
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
                return (0, "Invalid token format");

            if (!long.TryParse(parts[0], out var employeeId))
                return (0, "Invalid employee ID in token");

            if (!long.TryParse(parts[1], out var timestamp))
                return (0, "Invalid timestamp in token");

            var providedSignature = parts[2];

            // Verify signature
            var secretKey = _configuration["Jwt:SecretKey"] ?? "default-secret-key";
            var payload = $"{employeeId}:{timestamp}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);

            if (providedSignature != expectedSignature)
                return (0, "Invalid token signature");

            // Check expiration (default 72 hours)
            var expirationHours = _configuration.GetValue<int>("Application:OnboardingTokenExpirationHours", 72);
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var expirationTime = tokenTime.AddHours(expirationHours);

            if (DateTimeOffset.UtcNow > expirationTime)
                return (0, "Token has expired");

            return (employeeId, null);
        }
        catch (FormatException)
        {
            return (0, "Invalid token encoding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating onboarding token");
            return (0, "Token validation failed");
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return false;

        _repo.Remove(exists);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static EmployeeDto ToDto(Employee e) =>
        new EmployeeDto(
            e.Id,
            e.FullName,
            e.FirstName,
            e.LastName,
            e.PreferredName,
            e.Email,
            e.PersonalEmail,
            e.Phone,
            e.Phone2,
            e.Sex,
            e.DateOfBirth,
            e.MaritalStatus,
            e.Pronoun,
            e.PermanentAddress,
            e.CurrentAddress,
            e.NationalIdCountry,
            e.NationalIdNumber,
            e.NationalIdIssuedDate,
            e.NationalIdExpirationDate,
            e.NationalIdIssuedBy,
            e.SocialInsuranceNumber,
            e.TaxId,
            e.StartDate,
            e.Position?.Title,
            e.Department?.Name,
            e.JobLevel,
            e.EmployeeType,
            e.TimeType,
            e.Status,
            e.CreatedAt,
            e.UpdatedAt
        );
}
