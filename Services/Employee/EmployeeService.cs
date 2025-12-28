using EmployeeApi.Dtos;
using EmployeeApi.Helpers;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Data;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services.Employee;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _db;
    private readonly IMessageProducerService _messageProducer;
    private readonly IAuthService _authService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IOnboardingTokenService _tokenService;
    private readonly ILogger<EmployeeService> _logger;

    private const string SEND_EMAIL_QUEUE = "sendEmail";

    public EmployeeService(
        IEmployeeRepository repo,
        AppDbContext db,
        IMessageProducerService messageProducer,
        IAuthService authService,
        IEmailTemplateService emailTemplateService,
        IOnboardingTokenService tokenService,
        ILogger<EmployeeService> logger)
    {
        _repo = repo;
        _db = db;
        _messageProducer = messageProducer;
        _authService = authService;
        _emailTemplateService = emailTemplateService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null)
    {
        var list = await _repo.ListAsync(string.IsNullOrWhiteSpace(search) ? null :
            e => e.FullName.Contains(search!) || e.Email.Contains(search!));
        return list.Select(EmployeeMapper.ToDto);
    }

    public async Task<EmployeeDto?> GetOneAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : EmployeeMapper.ToDto(e);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
    {
        ValidateCreateInput(input);

        if (await _repo.ExistsByEmailAsync(input.Email.Trim()))
            throw new InvalidOperationException("Employee already exists");

        var entity = new Models.Employee
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
        return EmployeeMapper.ToDto(entity);
    }

    public async Task<EmployeeDto> CreateInitialProfileAsync(InitialProfileDto input)
    {
        await ValidateInitialProfileInput(input);

        var entity = new Models.Employee
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

        // Register auth account and send onboarding email
        var generatedPassword = await RegisterAuthAccountAsync(entity);
        await PublishOnboardingEmailEvent(entity, generatedPassword);

        return EmployeeMapper.ToDto(entity);
    }

    public async Task<EmployeeDto> CompleteOnboardingAsync(long employeeId, OnboardDto input)
    {
        var employee = await _repo.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        if (employee.Status == EmployeeStatus.Active.ToApiString())
            throw new InvalidOperationException("Onboarding has already been completed");

        EmployeeMapper.UpdateFromOnboardDto(employee, input);
        employee.Status = EmployeeStatus.Active.ToApiString();

        _repo.Update(employee);
        await SaveEducationRecordsAsync(employeeId, input.Education, replaceExisting: false);
        await SaveBankAccountAsync(employeeId, input.BankAccount, replaceExisting: false);
        await _repo.SaveChangesAsync();

        return EmployeeMapper.ToDto(employee);
    }

    public async Task<EmployeeDto> GetByOnboardingTokenAsync(string token)
    {
        var result = _tokenService.ValidateToken(token);
        if (!result.IsValid)
            throw new ArgumentException(result.ErrorMessage);

        var employee = await _repo.GetByIdWithDetailsAsync(result.EmployeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        return EmployeeMapper.ToDto(employee);
    }

    public async Task<EmployeeDto> SaveOnboardingProgressAsync(string token, OnboardDto input)
    {
        var result = _tokenService.ValidateToken(token);
        if (!result.IsValid)
            throw new ArgumentException(result.ErrorMessage);

        var employee = await _repo.GetByIdAsync(result.EmployeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        if (employee.Status == EmployeeStatus.Active.ToApiString())
            throw new InvalidOperationException("Onboarding has already been completed");

        EmployeeMapper.UpdateFromOnboardDto(employee, input);
        // Keep status as PENDING_ONBOARDING (don't complete)

        _repo.Update(employee);
        await SaveEducationRecordsAsync(result.EmployeeId, input.Education, replaceExisting: true);
        await SaveBankAccountAsync(result.EmployeeId, input.BankAccount, replaceExisting: true);
        await _repo.SaveChangesAsync();

        return EmployeeMapper.ToDto(employee);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return false;

        _repo.Remove(exists);
        await _repo.SaveChangesAsync();
        return true;
    }

    #region Private Helper Methods

    private static void ValidateCreateInput(CreateEmployeeDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required");
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required");
    }

    private async Task ValidateInitialProfileInput(InitialProfileDto input)
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
        var department = await _db.Departments.FindAsync(input.DepartmentId)
            ?? throw new ArgumentException($"Department with ID {input.DepartmentId} does not exist");

        var position = await _db.Positions.FindAsync(input.PositionId)
            ?? throw new ArgumentException($"Position with ID {input.PositionId} does not exist");

        if (input.ManagerId.HasValue)
        {
            var manager = await _repo.GetByIdAsync(input.ManagerId.Value)
                ?? throw new ArgumentException($"Manager with ID {input.ManagerId} does not exist");
        }
    }

    private async Task<string?> RegisterAuthAccountAsync(Models.Employee employee)
    {
        var generatedPassword = _authService.GenerateSecurePassword();
        var authResult = await _authService.RegisterAccountAsync(
            employee.Email,
            generatedPassword,
            "USER",
            employee.Id);

        if (!authResult.Success)
        {
            _logger.LogWarning(
                "Failed to create auth account for employee {EmployeeId}: {Error}",
                employee.Id, authResult.ErrorMessage);
            return null;
        }

        _logger.LogInformation(
            "Successfully created auth account for employee {EmployeeId}",
            employee.Id);
        return generatedPassword;
    }

    private async Task PublishOnboardingEmailEvent(Models.Employee employee, string? generatedPassword)
    {
        try
        {
            var personalEmail = employee.PersonalEmail ?? employee.Email;
            var onboardingLink = _tokenService.GenerateOnboardingLink(employee.Id);

            var emailData = new OnboardingEmailData(
                FullName: employee.FullName,
                WorkEmail: employee.Email,
                OnboardingLink: onboardingLink,
                StartDate: employee.StartDate?.ToString("MMMM dd, yyyy"),
                GeneratedPassword: generatedPassword
            );

            var emailEvent = new SendEmailEvent
            {
                EmailToSend = personalEmail,
                Subject = $"Welcome to the Team, {employee.FullName}! Complete Your Onboarding",
                HtmlContent = _emailTemplateService.GenerateOnboardingEmail(emailData)
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

    private async Task SaveEducationRecordsAsync(long employeeId, List<EducationDto>? educations, bool replaceExisting)
    {
        if (replaceExisting)
        {
            var existingEducations = _db.Educations.Where(e => e.EmployeeId == employeeId);
            _db.Educations.RemoveRange(existingEducations);
        }

        if (educations == null || educations.Count == 0)
            return;

        foreach (var edu in educations)
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

    private async Task SaveBankAccountAsync(long employeeId, BankAccountDto? bankAccountDto, bool replaceExisting)
    {
        if (replaceExisting)
        {
            var existingBankAccounts = _db.BankAccounts.Where(b => b.EmployeeId == employeeId);
            _db.BankAccounts.RemoveRange(existingBankAccounts);
        }

        if (bankAccountDto == null ||
            string.IsNullOrWhiteSpace(bankAccountDto.BankName) ||
            string.IsNullOrWhiteSpace(bankAccountDto.AccountNumber))
            return;

        var bankAccount = new BankAccount
        {
            EmployeeId = employeeId,
            BankName = bankAccountDto.BankName.Trim(),
            AccountNumber = bankAccountDto.AccountNumber.Trim(),
            AccountName = bankAccountDto.AccountName?.Trim()
        };
        await _db.BankAccounts.AddAsync(bankAccount);
    }

    #endregion
}

