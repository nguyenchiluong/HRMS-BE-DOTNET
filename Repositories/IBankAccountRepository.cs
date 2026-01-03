using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IBankAccountRepository : IRepository<BankAccount>
{
    /// <summary>
    /// Gets all bank accounts for a specific employee
    /// </summary>
    Task<IReadOnlyList<BankAccount>> GetByEmployeeIdAsync(long employeeId);
    
    /// <summary>
    /// Gets a specific bank account by account number, bank name, and employee ID
    /// </summary>
    Task<BankAccount?> GetByKeysAndEmployeeIdAsync(string accountNumber, string bankName, long employeeId);
    
    /// <summary>
    /// Gets a specific bank account by composite key (account number and bank name)
    /// </summary>
    Task<BankAccount?> GetByCompositeKeyAsync(string accountNumber, string bankName);
}
