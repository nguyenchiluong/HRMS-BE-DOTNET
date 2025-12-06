using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null);
    Task<EmployeeDto?> GetOneAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto input);
    Task<bool> DeleteAsync(int id);
}
