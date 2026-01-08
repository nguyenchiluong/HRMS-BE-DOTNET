using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IBankAccountRepository : IRepository<BankAccount>
{
    /// <summary>
    /// Gets all bank accounts for a specific employee
    /// </summary>
    Task<IReadOnlyList<BankAccount>> GetByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets the first bank account for a specific employee (for single account per employee)
    /// </summary>
    Task<BankAccount?> GetFirstByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets a specific bank account by bank name and employee ID
    /// </summary>
    Task<BankAccount?> GetByBankNameAndEmployeeIdAsync(string bankName, long employeeId);
    
    /// <summary>
    /// Gets a specific bank account by account number, bank name, and employee ID
    /// </summary>
    Task<BankAccount?> GetByKeysAndEmployeeIdAsync(string accountNumber, string bankName, long employeeId);
    
    /// <summary>
    /// Gets a specific bank account by composite key (account number and bank name)
    /// </summary>
    Task<BankAccount?> GetByCompositeKeyAsync(string accountNumber, string bankName);
    
    /// <summary>
    /// Gets a bank account by id and employee ID to ensure it belongs to the employee
    /// </summary>
    Task<BankAccount?> GetByIdAndEmployeeIdAsync(long id, long employeeId);
}
