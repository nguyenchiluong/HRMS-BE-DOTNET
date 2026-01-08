using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IBankAccountService
{
    /// <summary>
    /// Gets all bank accounts for a specific employee
    /// </summary>
    Task<IReadOnlyList<BankAccountRecordDto>> GetAllByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets the first bank account for a specific employee (for single account per employee)
    /// </summary>
    Task<BankAccountRecordDto?> GetFirstByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets a specific bank account if it belongs to the employee
    /// </summary>
    Task<BankAccountRecordDto?> GetByKeysAsync(string accountNumber, string bankName, long employeeId);
    
    /// <summary>
    /// Gets a bank account by bank name for a specific employee
    /// </summary>
    Task<BankAccountRecordDto?> GetByBankNameAsync(string bankName, long employeeId);
    
    /// <summary>
    /// Creates or updates a bank account for an employee (upsert by bankName)
    /// </summary>
    Task<BankAccountRecordDto> UpsertByBankNameAsync(string bankName, long employeeId, CreateBankAccountDto dto);
    
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
    
    /// <summary>
    /// Deletes a bank account by bank name if it belongs to the employee
    /// </summary>
    Task<bool> DeleteByBankNameAsync(string bankName, long employeeId);
    
    /// <summary>
    /// Gets a bank account by id if it belongs to the employee
    /// </summary>
    Task<BankAccountRecordDto?> GetByIdAsync(long id, long employeeId);
    
    /// <summary>
    /// Updates a bank account by id if it belongs to the employee
    /// </summary>
    Task<BankAccountRecordDto?> UpdateByIdAsync(long id, long employeeId, UpdateBankAccountDto dto);
    
    /// <summary>
    /// Deletes a bank account by id if it belongs to the employee
    /// </summary>
    Task<bool> DeleteByIdAsync(long id, long employeeId);
}
