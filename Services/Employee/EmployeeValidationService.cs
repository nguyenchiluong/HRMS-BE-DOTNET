using EmployeeApi.Dtos;
using EmployeeApi.Data;
using EmployeeApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Services.Employee;

/// <summary>
/// Service for validating employee-related data
/// </summary>
public class EmployeeValidationService
{
    private readonly AppDbContext _db;
    private readonly IEmployeeRepository _repo;

    public EmployeeValidationService(AppDbContext db, IEmployeeRepository repo)
    {
        _db = db;
        _repo = repo;
    }

    public void ValidateCreateInput(CreateEmployeeDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName))
            throw new ArgumentException("FullName is required");
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email is required");
    }

    public async Task ValidateInitialProfileInput(InitialProfileDto input, string workEmail)
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
            await ValidateManagerAsync(input.ManagerId.Value);
        }

        if (input.HrId.HasValue)
        {
            await ValidateHrAsync(input.HrId.Value);
        }
    }

    public async Task ValidateManagerAsync(long managerId, long? excludeEmployeeId = null)
    {
        var manager = await _db.Employees
            .Include(e => e.JobLevel)
            .FirstOrDefaultAsync(e => e.Id == managerId);

        if (manager == null)
            throw new ArgumentException($"Manager with ID {managerId} does not exist");

        if (manager.Status != "ACTIVE")
            throw new ArgumentException($"Manager with ID {managerId} is not active");

        var isManager = manager.JobLevel != null && (
            manager.JobLevel.Name == "Manager" ||
            manager.JobLevel.Name == "Director" ||
            manager.JobLevel.Name == "Principal"
        );

        if (!isManager)
            throw new ArgumentException($"Employee with ID {managerId} cannot serve as a manager");

        if (excludeEmployeeId.HasValue && manager.Id == excludeEmployeeId.Value)
            throw new ArgumentException("Employee cannot be their own manager");
    }

    public async Task ValidateHrAsync(long hrId)
    {
        var hr = await _db.Employees
            .Include(e => e.Position)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == hrId);

        if (hr == null)
            throw new ArgumentException($"HR personnel with ID {hrId} does not exist");

        if (hr.Status != "ACTIVE")
            throw new ArgumentException($"HR personnel with ID {hrId} is not active");

        var isHr = hr.DepartmentId == 6 ||
            (hr.Position != null && hr.Position.Title.Contains("HR"));

        if (!isHr)
            throw new ArgumentException($"Employee with ID {hrId} is not HR personnel");
    }

    public async Task ValidateReassignSupervisorsAsync(long employeeId, ReassignSupervisorsDto input)
    {
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee == null)
            throw new KeyNotFoundException("Employee not found");

        var errors = new List<string>();

        if (input.ManagerId.HasValue)
        {
            try
            {
                await ValidateManagerAsync(input.ManagerId.Value, employeeId);
            }
            catch (ArgumentException ex)
            {
                errors.Add(ex.Message.Contains("their own manager") 
                    ? "Employee cannot be their own manager" 
                    : "Manager ID is invalid");
            }
        }

        if (input.HrId.HasValue)
        {
            try
            {
                await ValidateHrAsync(input.HrId.Value);
            }
            catch (ArgumentException)
            {
                errors.Add("HR ID is invalid");
            }
        }

        if (errors.Count > 0)
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
    }
}

