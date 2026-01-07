using EmployeeApi.Dtos;
using EmployeeApi.Helpers;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Data;
using EmployeeApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeApi.Services.Employee;

public class EmployeeWriteService : IEmployeeWriteService
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _db;
    private readonly IMessageProducerService _messageProducer;
    private readonly IAuthService _authService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IOnboardingTokenService _tokenService;
    private readonly EmployeeValidationService _validationService;
    private readonly ILogger<EmployeeWriteService> _logger;

    private const string SEND_EMAIL_QUEUE = "sendEmail";

    public EmployeeWriteService(
        IEmployeeRepository repo,
        AppDbContext db,
        IMessageProducerService messageProducer,
        IAuthService authService,
        IEmailTemplateService emailTemplateService,
        IOnboardingTokenService tokenService,
        EmployeeValidationService validationService,
        ILogger<EmployeeWriteService> logger)
    {
        _repo = repo;
        _db = db;
        _messageProducer = messageProducer;
        _authService = authService;
        _emailTemplateService = emailTemplateService;
        _tokenService = tokenService;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
    {
        _validationService.ValidateCreateInput(input);

        var emailExists = await _repo.ExistsByEmailAsync(input.Email.Trim());
        if (emailExists)
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
        var workEmail = GenerateWorkEmail(input.FullName);
        await _validationService.ValidateInitialProfileInput(input, workEmail);

        var entity = new Models.Employee
        {
            Id = await _repo.GetNextIdAsync(),
            FullName = input.FullName.Trim(),
            Email = workEmail,
            PersonalEmail = input.PersonalEmail.Trim(),
            PositionId = input.PositionId,
            JobLevelId = input.JobLevelId,
            DepartmentId = input.DepartmentId,
            EmploymentTypeId = input.EmploymentTypeId,
            TimeTypeId = input.TimeTypeId,
            StartDate = input.StartDate,
            ManagerId = input.ManagerId,
            HrId = input.HrId,
            Status = EmployeeStatus.PendingOnboarding.ToApiString(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

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

    public async Task<EmployeeDto> UpdateProfileAsync(long employeeId, UpdateProfileDto input)
    {
        var employee = await _repo.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        if (!string.IsNullOrWhiteSpace(input.FirstName))
            employee.FirstName = input.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(input.LastName))
            employee.LastName = input.LastName.Trim();
        if (input.PreferredName != null)
            employee.PreferredName = string.IsNullOrWhiteSpace(input.PreferredName) ? null : input.PreferredName.Trim();
        if (input.Sex != null)
            employee.Sex = input.Sex;
        if (input.DateOfBirth.HasValue)
            employee.DateOfBirth = input.DateOfBirth;
        if (input.MaritalStatus != null)
            employee.MaritalStatus = input.MaritalStatus;
        if (input.Pronoun != null)
            employee.Pronoun = input.Pronoun;
        if (input.PersonalEmail != null)
            employee.PersonalEmail = string.IsNullOrWhiteSpace(input.PersonalEmail) ? null : input.PersonalEmail.Trim();
        if (input.Phone != null)
            employee.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        if (input.Phone2 != null)
            employee.Phone2 = string.IsNullOrWhiteSpace(input.Phone2) ? null : input.Phone2.Trim();

        if (input.PermanentAddress != null)
            employee.PermanentAddress = string.IsNullOrWhiteSpace(input.PermanentAddress) ? null : input.PermanentAddress.Trim();
        if (input.CurrentAddress != null)
            employee.CurrentAddress = string.IsNullOrWhiteSpace(input.CurrentAddress) ? null : input.CurrentAddress.Trim();

        if (input.NationalId != null)
        {
            employee.NationalIdCountry = input.NationalId.Country;
            employee.NationalIdNumber = input.NationalId.Number;
            employee.NationalIdIssuedDate = input.NationalId.IssuedDate;
            employee.NationalIdExpirationDate = input.NationalId.ExpirationDate;
            employee.NationalIdIssuedBy = input.NationalId.IssuedBy;
        }

        if (input.SocialInsuranceNumber != null)
            employee.SocialInsuranceNumber = string.IsNullOrWhiteSpace(input.SocialInsuranceNumber) ? null : input.SocialInsuranceNumber.Trim();
        if (input.TaxId != null)
            employee.TaxId = string.IsNullOrWhiteSpace(input.TaxId) ? null : input.TaxId.Trim();

        employee.UpdatedAt = DateTime.Now;

        _repo.Update(employee);
        await _repo.SaveChangesAsync();

        return EmployeeMapper.ToDto(employee);
    }

    public async Task ReassignSupervisorsAsync(long employeeId, ReassignSupervisorsDto input)
    {
        await _validationService.ValidateReassignSupervisorsAsync(employeeId, input);

        var employee = await _db.Employees.FindAsync(employeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        employee.ManagerId = input.ManagerId;
        employee.HrId = input.HrId;
        employee.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    #region Private Helper Methods

    private static string GenerateWorkEmail(string fullName)
    {
        var normalized = fullName.Trim().ToLowerInvariant();
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", ".");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9.]", "");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\.+", ".");
        normalized = normalized.Trim('.');
        return $"{normalized}@hrms.com";
    }

    private async Task<string?> RegisterAuthAccountAsync(Models.Employee employee)
    {
        var generatedPassword = _authService.GenerateSecurePassword();
        var role = employee.PositionId == 9 ? "ADMIN" : "USER";

        var authResult = await _authService.RegisterAccountAsync(
            employee.Email,
            generatedPassword,
            role,
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
            var personalEmail = employee.PersonalEmail
                ?? throw new InvalidOperationException("Personal email is required to send onboarding email");

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
                "Published onboarding email event for employee {EmployeeId} to personal email {PersonalEmail}",
                employee.Id, personalEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish onboarding email event for employee {EmployeeId}",
                employee.Id);
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

