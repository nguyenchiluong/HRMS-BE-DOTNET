using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IBankAccountService
{
    /// <summary>
    /// Gets all bank accounts for a specific employee
    /// </summary>
    Task<IReadOnlyList<BankAccountRecordDto>> GetAllByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets a specific bank account if it belongs to the employee
    /// </summary>
    Task<BankAccountRecordDto?> GetByKeysAsync(string accountNumber, string bankName, long employeeId);
    
    /// <summary>
    /// Creates a new bank account for an employee
    /// </summary>
    Task<BankAccountRecordDto> CreateAsync(long employeeId, CreateBankAccountDto dto);
    
    /// <summary>
    /// Updates an existing bank account if it belongs to the employee
    /// </summary>
    Task<BankAccountRecordDto?> UpdateAsync(string accountNumber, string bankName, long employeeId, UpdateBankAccountDto dto);
    
    /// <summary>
    /// Deletes a bank account if it belongs to the employee
    /// </summary>
    Task<bool> DeleteAsync(string accountNumber, string bankName, long employeeId);
}
