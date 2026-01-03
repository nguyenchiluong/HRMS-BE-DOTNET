using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IEducationService
{
    /// <summary>
    /// Gets all education records for a specific employee
    /// </summary>
    Task<IReadOnlyList<EducationRecordDto>> GetAllByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets a specific education record if it belongs to the employee
    /// </summary>
    Task<EducationRecordDto?> GetByIdAsync(long id, long employeeId);
    
    /// <summary>
    /// Creates a new education record for an employee
    /// </summary>
    Task<EducationRecordDto> CreateAsync(long employeeId, CreateEducationDto dto);
    
    /// <summary>
    /// Updates an existing education record if it belongs to the employee
    /// </summary>
    Task<EducationRecordDto?> UpdateAsync(long id, long employeeId, UpdateEducationDto dto);
    
    /// <summary>
    /// Deletes an education record if it belongs to the employee
    /// </summary>
    Task<bool> DeleteAsync(long id, long employeeId);
}
