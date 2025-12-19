using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;
using EmployeeApi.Data;

namespace EmployeeApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly AppDbContext _db;

    public EmployeeService(IEmployeeRepository repo, AppDbContext db)
    {
        _repo = repo;
        _db = db;
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
            Status = input.Status ?? "PENDING_ONBOARDING",
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
            Status = "PENDING_ONBOARDING",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<EmployeeDto> CompleteOnboardingAsync(long employeeId, OnboardDto input)
    {
        var employee = await _repo.GetByIdAsync(employeeId);
        if (employee is null)
            throw new KeyNotFoundException("Employee not found");

        if (employee.Status == "ACTIVE")
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
        employee.Status = "ACTIVE";
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
