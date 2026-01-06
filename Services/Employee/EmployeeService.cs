using EmployeeApi.Dtos;
using EmployeeApi.Helpers;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Data;
using Microsoft.EntityFrameworkCore;
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

    public async Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(long managerId)
    {
        var list = await _repo.GetByManagerIdAsync(managerId);
        return list.Select(EmployeeMapper.ToDto);
    }

    public async Task<EmployeeDto?> GetOneAsync(long id)
    {
        // Load related entities so names (position/department) are populated
        var e = await _repo.GetByIdWithDetailsAsync(id);
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
        // Generate work email from full name
        var workEmail = GenerateWorkEmail(input.FullName);

        await ValidateInitialProfileInput(input, workEmail);

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

        // Register auth account (using work email) and send onboarding email (to personal email)
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

    public async Task<EmployeePaginatedResponse<FilteredEmployeeDto>> GetFilteredAsync(
        string? searchTerm,
        List<string>? statuses,
        List<string>? departments,
        List<string>? positions,
        List<string>? jobLevels,
        List<string>? employmentTypes,
        List<string>? timeTypes,
        int page,
        int pageSize)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 14;
        if (pageSize > 100) pageSize = 100;

        var (employees, totalCount) = await _repo.GetFilteredAsync(
            searchTerm,
            statuses,
            departments,
            positions,
            jobLevels,
            employmentTypes,
            timeTypes,
            page,
            pageSize);

        var filteredDtos = employees.Select(e => new FilteredEmployeeDto(
            Id: e.Id.ToString(),
            FullName: e.FullName,
            WorkEmail: e.Email,
            Position: e.Position?.Title,
            JobLevel: e.JobLevel?.Name,
            Department: e.Department?.Name,
            Status: MapStatusToUserFriendly(e.Status),
            EmploymentType: e.EmploymentType?.Name,
            TimeType: e.TimeType?.Name
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagination = new EmployeePaginationDto(
            CurrentPage: page,
            PageSize: pageSize,
            TotalItems: totalCount,
            TotalPages: totalPages
        );

        return new EmployeePaginatedResponse<FilteredEmployeeDto>(filteredDtos, pagination);
    }

    public async Task<EmployeeStatsDto> GetStatsAsync()
    {
        var (total, onboarding, resigned, managers) = await _repo.GetStatsAsync();
        return new EmployeeStatsDto(total, onboarding, resigned, managers);
    }

    #region Private Helper Methods

    /// <summary>
    /// Maps database status to user-friendly format
    /// </summary>
    private static string MapStatusToUserFriendly(string? status)
    {
        return status?.ToUpper() switch
        {
            "PENDING_ONBOARDING" => "Pending",
            "ACTIVE" => "Active",
            "INACTIVE" => "Inactive",
            _ => status ?? "Unknown"
        };
    }

    private static void ValidateCreateInput(CreateEmployeeDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required");
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required");
    }

    private async Task ValidateInitialProfileInput(InitialProfileDto input, string workEmail)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required");
        if (string.IsNullOrWhiteSpace(input.PersonalEmail))
            throw new ArgumentException("PersonalEmail is required");

        // Check if generated work email already exists
        if (await _repo.ExistsByEmailAsync(workEmail))
            throw new InvalidOperationException($"Employee with work email {workEmail} already exists");

        // Validate foreign keys
        var department = await _db.Departments.FindAsync(input.DepartmentId)
            ?? throw new ArgumentException($"Department with ID {input.DepartmentId} does not exist");

        var position = await _db.Positions.FindAsync(input.PositionId)
            ?? throw new ArgumentException($"Position with ID {input.PositionId} does not exist");

        var jobLevel = await _db.JobLevels.FindAsync(input.JobLevelId)
            ?? throw new ArgumentException($"JobLevel with ID {input.JobLevelId} does not exist");

        var employmentType = await _db.EmploymentTypes.FindAsync(input.EmploymentTypeId)
            ?? throw new ArgumentException($"EmploymentType with ID {input.EmploymentTypeId} does not exist");

        var timeType = await _db.TimeTypes.FindAsync(input.TimeTypeId)
            ?? throw new ArgumentException($"TimeType with ID {input.TimeTypeId} does not exist");

        if (input.ManagerId.HasValue)
        {
            var manager = await _db.Employees
                .Include(e => e.JobLevel)
                .FirstOrDefaultAsync(e => e.Id == input.ManagerId.Value)
                ?? throw new ArgumentException($"Manager with ID {input.ManagerId} does not exist");

            if (manager.Status != "ACTIVE")
                throw new ArgumentException($"Manager with ID {input.ManagerId} is not active");

            // Check if employee can serve as manager - only based on job levels
            var isManager = manager.JobLevel != null && (
                manager.JobLevel.Name == "Manager" ||
                manager.JobLevel.Name == "Director" ||
                manager.JobLevel.Name == "Principal"
            );

            if (!isManager)
                throw new ArgumentException($"Employee with ID {input.ManagerId} cannot serve as a manager");
        }

        if (input.HrId.HasValue)
        {
            var hr = await _db.Employees
                .Include(e => e.Position)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == input.HrId.Value)
                ?? throw new ArgumentException($"HR personnel with ID {input.HrId} does not exist");

            if (hr.Status != "ACTIVE")
                throw new ArgumentException($"HR personnel with ID {input.HrId} is not active");

            // Check if employee is HR personnel
            var isHr = hr.DepartmentId == 6 || // Department ID 6 = "Human Resources"
                (hr.Position != null && (hr.Position.Title.Contains("HR")));

            if (!isHr)
                throw new ArgumentException($"Employee with ID {input.HrId} is not HR personnel");
        }
    }

    /// <summary>
    /// Generates a work email from the full name (e.g., "John Doe" → "john.doe@hrms.com")
    /// </summary>
    private static string GenerateWorkEmail(string fullName)
    {
        // Normalize: lowercase, replace spaces with dots, remove special characters
        var normalized = fullName.Trim().ToLowerInvariant();

        // Replace multiple spaces with single space, then replace space with dot
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", ".");

        // Remove any characters that are not letters, numbers, or dots
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9.]", "");

        // Remove consecutive dots and trim dots from start/end
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\.+", ".");
        normalized = normalized.Trim('.');

        return $"{normalized}@hrms.com";
    }

    private async Task<string?> RegisterAuthAccountAsync(Models.Employee employee)
    {
        var generatedPassword = _authService.GenerateSecurePassword();

        // HR Specialist (Position ID 9) should have Admin role
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
            // Personal email is required for initial profile creation
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

    public async Task<EmployeeDto> UpdateProfileAsync(long employeeId, UpdateProfileDto input)
    {
        var employee = await _repo.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        // Update personal details
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

        // Update addresses
        if (input.PermanentAddress != null)
            employee.PermanentAddress = string.IsNullOrWhiteSpace(input.PermanentAddress) ? null : input.PermanentAddress.Trim();
        if (input.CurrentAddress != null)
            employee.CurrentAddress = string.IsNullOrWhiteSpace(input.CurrentAddress) ? null : input.CurrentAddress.Trim();

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
        if (input.SocialInsuranceNumber != null)
            employee.SocialInsuranceNumber = string.IsNullOrWhiteSpace(input.SocialInsuranceNumber) ? null : input.SocialInsuranceNumber.Trim();
        if (input.TaxId != null)
            employee.TaxId = string.IsNullOrWhiteSpace(input.TaxId) ? null : input.TaxId.Trim();

        employee.UpdatedAt = DateTime.Now;

        _repo.Update(employee);
        await _repo.SaveChangesAsync();

        return EmployeeMapper.ToDto(employee);
    }

    public async Task<IEnumerable<ManagerOrHrDto>> GetManagersAsync(string? search = null)
    {
        // Build query for managers - only based on job levels
        IQueryable<Models.Employee> query = _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .Include(e => e.JobLevel)
            .Include(e => e.EmploymentType)
            .Include(e => e.TimeType)
            .AsNoTracking()
            .Where(e => e.Status == "ACTIVE" &&
                e.JobLevel != null && (
                    e.JobLevel.Name == "Manager" ||
                    e.JobLevel.Name == "Director" ||
                    e.JobLevel.Name == "Principal"
                ));

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            long? searchId = null;
            if (long.TryParse(search, out var parsedId))
            {
                searchId = parsedId;
            }

            query = query.Where(e =>
                (searchId.HasValue && e.Id == searchId.Value) ||
                e.FullName.ToLower().Contains(searchLower) ||
                e.Email.ToLower().Contains(searchLower)
            );
        }

        var managers = await query.OrderBy(e => e.FullName).ToListAsync();

        return managers.Select(e => new ManagerOrHrDto(
            Id: e.Id,
            FullName: e.FullName,
            WorkEmail: e.Email,
            Position: e.Position?.Title,
            PositionId: e.PositionId,
            JobLevel: e.JobLevel?.Name,
            JobLevelId: e.JobLevelId,
            Department: e.Department?.Name,
            DepartmentId: e.DepartmentId,
            EmploymentType: e.EmploymentType?.Name,
            EmploymentTypeId: e.EmploymentTypeId,
            TimeType: e.TimeType?.Name,
            TimeTypeId: e.TimeTypeId
        ));
    }

    public async Task<IEnumerable<ManagerOrHrDto>> GetHrPersonnelAsync(string? search = null)
    {
        // Build query for HR personnel
        IQueryable<Models.Employee> query = _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .Include(e => e.JobLevel)
            .Include(e => e.EmploymentType)
            .Include(e => e.TimeType)
            .AsNoTracking()
            .Where(e => e.Status == "ACTIVE" && (
                // Employees in HR department (Department ID 6 = "Human Resources")
                e.DepartmentId == 6 ||
                // Employees with HR-related positions
                (e.Position != null && (
                    e.Position.Title.Contains("HR") ||
                    e.Position.Title == "HR Specialist" ||
                    e.Position.Title == "HR Manager" ||
                    e.Position.Title == "HR Coordinator" ||
                    e.Position.Title == "HR Director" ||
                    e.Position.Title == "HR Business Partner"
                ))
            ));

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            long? searchId = null;
            if (long.TryParse(search, out var parsedId))
            {
                searchId = parsedId;
            }

            query = query.Where(e =>
                (searchId.HasValue && e.Id == searchId.Value) ||
                e.FullName.ToLower().Contains(searchLower) ||
                e.Email.ToLower().Contains(searchLower)
            );
        }

        var hrPersonnel = await query.OrderBy(e => e.FullName).ToListAsync();

        return hrPersonnel.Select(e => new ManagerOrHrDto(
            Id: e.Id,
            FullName: e.FullName,
            WorkEmail: e.Email,
            Position: e.Position?.Title,
            PositionId: e.PositionId,
            JobLevel: e.JobLevel?.Name,
            JobLevelId: e.JobLevelId,
            Department: e.Department?.Name,
            DepartmentId: e.DepartmentId,
            EmploymentType: e.EmploymentType?.Name,
            EmploymentTypeId: e.EmploymentTypeId,
            TimeType: e.TimeType?.Name,
            TimeTypeId: e.TimeTypeId
        ));
    }

    #endregion
}

