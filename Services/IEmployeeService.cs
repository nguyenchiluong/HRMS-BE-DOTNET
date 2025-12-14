using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search = null);
    Task<EmployeeDto?> GetOneAsync(long id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto input);
    Task<bool> DeleteAsync(long id);
}
