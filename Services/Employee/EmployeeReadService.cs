using EmployeeApi.Dtos;
using EmployeeApi.Helpers;
using EmployeeApi.Repositories;
using EmployeeApi.Data;
using EmployeeApi.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Services.Employee;

public class EmployeeReadService : IEmployeeReadService
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _db;
    private readonly IOnboardingTokenService _tokenService;

    public EmployeeReadService(
        IEmployeeRepository repo,
        AppDbContext db,
        IOnboardingTokenService tokenService)
    {
        _repo = repo;
        _db = db;
        _tokenService = tokenService;
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
        var e = await _repo.GetByIdWithDetailsAsync(id);
        return e is null ? null : EmployeeMapper.ToDto(e);
    }

    public async Task<OnboardDto> GetByOnboardingTokenAsync(string token)
    {
        var result = _tokenService.ValidateToken(token);
        if (!result.IsValid)
            throw new ArgumentException(result.ErrorMessage);

        var employee = await _repo.GetByIdWithDetailsAsync(result.EmployeeId)
            ?? throw new KeyNotFoundException("Employee not found");

        // Fetch education records
        var educations = await _db.Educations
            .Where(e => e.EmployeeId == result.EmployeeId)
            .AsNoTracking()
            .ToListAsync();

        var educationDtos = educations.Select(e => new EducationDto
        {
            Degree = e.Degree,
            FieldOfStudy = e.FieldOfStudy,
            Country = e.Country,
            Institution = e.Institution,
            StartYear = e.StartYear,
            EndYear = e.EndYear,
            Gpa = e.Gpa
        }).ToList();

        // Fetch bank account
        var bankAccount = await _db.BankAccounts
            .Where(b => b.EmployeeId == result.EmployeeId)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        BankAccountDto? bankAccountDto = null;
        if (bankAccount != null)
        {
            bankAccountDto = new BankAccountDto
            {
                BankName = bankAccount.BankName,
                AccountNumber = bankAccount.AccountNumber,
                AccountName = bankAccount.AccountName,
                SwiftCode = bankAccount.SwiftCode,
                BranchCode = bankAccount.BranchCode
            };
        }

        // Map employee to OnboardDto format
        return new OnboardDto
        {
            // Personal details
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            PreferredName = employee.PreferredName,
            Sex = employee.Sex,
            DateOfBirth = employee.DateOfBirth,
            MaritalStatus = employee.MaritalStatus,
            Pronoun = employee.Pronoun,
            PersonalEmail = employee.PersonalEmail,
            Phone = employee.Phone,
            Phone2 = employee.Phone2,

            // Address
            PermanentAddress = employee.PermanentAddress,
            CurrentAddress = employee.CurrentAddress,

            // National ID
            NationalId = employee.NationalIdNumber != null || employee.NationalIdCountry != null
                ? new NationalIdDto
                {
                    Country = employee.NationalIdCountry,
                    Number = employee.NationalIdNumber,
                    IssuedDate = employee.NationalIdIssuedDate,
                    ExpirationDate = employee.NationalIdExpirationDate,
                    IssuedBy = employee.NationalIdIssuedBy
                }
                : null,

            // Social Insurance & Tax
            SocialInsuranceNumber = employee.SocialInsuranceNumber,
            TaxId = employee.TaxId,

            // Education history
            Education = educationDtos.Count > 0 ? educationDtos : null,

            // Bank account
            BankAccount = bankAccountDto,

            // Comment (not stored in employee table, so always null)
            Comment = null
        };
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
            TimeType: e.TimeType?.Name,
            ManagerId: e.ManagerId,
            ManagerName: e.Manager?.FullName,
            ManagerEmail: e.Manager?.Email,
            HrId: e.HrId,
            HrName: e.Hr?.FullName,
            HrEmail: e.Hr?.Email
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

    public async Task<IEnumerable<ManagerOrHrDto>> GetManagersAsync(string? search = null)
    {
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
        IQueryable<Models.Employee> query = _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .Include(e => e.JobLevel)
            .Include(e => e.EmploymentType)
            .Include(e => e.TimeType)
            .AsNoTracking()
            .Where(e => e.Status == "ACTIVE" && (
                e.DepartmentId == 6 ||
                (e.Position != null && (
                    e.Position.Title.Contains("HR") ||
                    e.Position.Title == "HR Specialist" ||
                    e.Position.Title == "HR Manager" ||
                    e.Position.Title == "HR Coordinator" ||
                    e.Position.Title == "HR Director" ||
                    e.Position.Title == "HR Business Partner"
                ))
            ));

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

    public async Task<bool> IsHrOrAdminAsync(long employeeId)
    {
        var employee = await _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null || employee.Status != "ACTIVE")
            return false;

        if (employee.DepartmentId == 6)
            return true;

        if (employee.Position != null && employee.Position.Title.Contains("HR"))
            return true;

        if (employee.PositionId == 9)
            return true;

        return false;
    }

    public async Task<bool> IsAdminAsync(long employeeId)
    {
        var employee = await _db.Employees
            .Include(e => e.Position)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null || employee.Status != "ACTIVE")
            return false;

        if (employee.PositionId == 9)
            return true;

        return false;
    }

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
}

