using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    public EmployeeService(IEmployeeRepository repo) => _repo = repo;

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null)
    {
        var list = await _repo.ListAsync(string.IsNullOrWhiteSpace(search) ? null :
            e => (e.FirstName + " " + e.LastName).Contains(search!) || (e.Email ?? "").Contains(search!));
        return list.Select(ToDto);
    }

    public async Task<EmployeeDto?> GetOneAsync(long id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : ToDto(e);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FirstName))
            throw new ArgumentException("FirstName is required");
        if (string.IsNullOrWhiteSpace(input.LastName))
            throw new ArgumentException("LastName is required");

        if (!string.IsNullOrWhiteSpace(input.Email) && await _repo.ExistsByEmailAsync(input.Email.Trim()))
            throw new InvalidOperationException("Employee already exists");

        var entity = new Employee
        {
            FirstName = input.FirstName.Trim(),
            LastName = input.LastName.Trim(),
            Email = input.Email?.Trim(),
            Phone = input.Phone,
            HireDate = input.HireDate,
            BirthDate = input.BirthDate,
            PositionId = input.PositionId,
            DepartmentId = input.DepartmentId,
            ManagerId = input.ManagerId,
            IsActive = input.IsActive,
            JobStatus = input.JobStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
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
            e.FirstName,
            e.LastName,
            e.Email,
            e.Phone,
            e.HireDate,
            e.BirthDate,
            e.Position?.Title,
            e.Department?.Name,
            e.IsActive,
            e.JobStatus,
            e.CreatedAt,
            e.UpdatedAt
        );
}
