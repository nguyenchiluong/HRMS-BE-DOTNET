using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly AppDbContext _context;

    public BankAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount?> GetByIdAsync(long id)
    {
        // BankAccount uses composite key, so this method isn't applicable
        throw new NotImplementedException("Use GetByCompositeKeyAsync for BankAccount");
    }

    public async Task<IReadOnlyList<BankAccount>> ListAsync(Expression<Func<BankAccount, bool>>? predicate = null)
    {
        var query = _context.BankAccounts.AsQueryable();
        
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        
        return await query.ToListAsync();
    }

    public async Task AddAsync(BankAccount entity)
    {
        await _context.BankAccounts.AddAsync(entity);
    }

    public void Update(BankAccount entity)
    {
        _context.BankAccounts.Update(entity);
    }

    public void Remove(BankAccount entity)
    {
        _context.BankAccounts.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<BankAccount>> GetByEmployeeIdAsync(long employeeId)
    {
        return await _context.BankAccounts
            .Where(b => b.EmployeeId == employeeId)
            .OrderBy(b => b.BankName)
            .ThenBy(b => b.AccountNumber)
            .ToListAsync();
    }

    public async Task<BankAccount?> GetByKeysAndEmployeeIdAsync(string accountNumber, string bankName, long employeeId)
    {
        var query = _context.BankAccounts
            .Where(b => b.AccountNumber == accountNumber && b.BankName == bankName);
        
        // If employeeId is 0, it means admin access - skip employee filter
        if (employeeId > 0)
        {
            query = query.Where(b => b.EmployeeId == employeeId);
        }
        
        return await query.FirstOrDefaultAsync();
    }

    public async Task<BankAccount?> GetByCompositeKeyAsync(string accountNumber, string bankName)
    {
        return await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.AccountNumber == accountNumber && b.BankName == bankName);
    }
}
