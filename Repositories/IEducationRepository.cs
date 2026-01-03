using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IEducationRepository : IRepository<Education>
{
    /// <summary>
    /// Gets all education records for a specific employee
    /// </summary>
    Task<IReadOnlyList<Education>> GetByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets a specific education record if it belongs to the employee
    /// </summary>
    Task<Education?> GetByIdAndEmployeeIdAsync(long id, long employeeId);
}
