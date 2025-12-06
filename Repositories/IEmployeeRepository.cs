using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<bool> ExistsByEmailAsync(string email);
}
