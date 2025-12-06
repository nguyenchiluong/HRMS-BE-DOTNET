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
            e => e.FullName.Contains(search!) || e.Email.Contains(search!));
        return list.Select(ToDto);
    }

    public async Task<EmployeeDto?> GetOneAsync(int id)
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
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            Position = input.Position,
            StartDate = input.StartDate,
            Status = input.Status,
            JobLevel = input.JobLevel,
            Department = input.Department,
            EmploymentType = input.EmploymentType,
            TimeType = input.TimeType,
            LastUpdated = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return false;

        _repo.Remove(exists);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static EmployeeDto ToDto(Employee e) =>
        new EmployeeDto(
            e.Id, e.FullName, e.Email, e.Position, e.StartDate,
            e.Status, e.JobLevel, e.Department,
            e.EmploymentType, e.TimeType, e.LastUpdated
        );
}
