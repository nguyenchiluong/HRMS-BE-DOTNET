using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<Employee?> GetByEmailAsync(string email);
    Task<long> GetNextIdAsync();
    Task<Employee?> GetByIdWithDetailsAsync(long id);
}
