using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        IEmployeeRepository employeeRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<IReadOnlyList<BankAccountRecordDto>> GetAllByEmployeeIdAsync(long employeeId)
    {
        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        var bankAccounts = await _bankAccountRepository.GetByEmployeeIdAsync(employeeId);
        return bankAccounts.Select(MapToDto).ToList();
    }

    public async Task<BankAccountRecordDto?> GetByKeysAsync(string accountNumber, string bankName, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByKeysAndEmployeeIdAsync(accountNumber, bankName, employeeId);
        return bankAccount == null ? null : MapToDto(bankAccount);
    }

    public async Task<BankAccountRecordDto> CreateAsync(long employeeId, CreateBankAccountDto dto)
    {
        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        // Check if bank account already exists
        var existing = await _bankAccountRepository.GetByCompositeKeyAsync(dto.AccountNumber, dto.BankName);
        if (existing != null)
        {
            throw new InvalidOperationException($"Bank account with number '{dto.AccountNumber}' at '{dto.BankName}' already exists");
        }

        var bankAccount = new BankAccount
        {
            EmployeeId = employeeId,
            AccountNumber = dto.AccountNumber,
            BankName = dto.BankName,
            AccountName = dto.AccountName
        };

        await _bankAccountRepository.AddAsync(bankAccount);
        await _bankAccountRepository.SaveChangesAsync();

        return MapToDto(bankAccount);
    }

    public async Task<BankAccountRecordDto?> UpdateAsync(string accountNumber, string bankName, long employeeId, UpdateBankAccountDto dto)
    {
        var bankAccount = await _bankAccountRepository.GetByKeysAndEmployeeIdAsync(accountNumber, bankName, employeeId);
        if (bankAccount == null)
        {
            return null;
        }

        // Check if trying to change composite key to an existing account
        if ((dto.AccountNumber != null && dto.AccountNumber != accountNumber) || 
            (dto.BankName != null && dto.BankName != bankName))
        {
            var newAccountNumber = dto.AccountNumber ?? accountNumber;
            var newBankName = dto.BankName ?? bankName;
            
            var existing = await _bankAccountRepository.GetByCompositeKeyAsync(newAccountNumber, newBankName);
            if (existing != null && (existing.AccountNumber != accountNumber || existing.BankName != bankName))
            {
                throw new InvalidOperationException($"Bank account with number '{newAccountNumber}' at '{newBankName}' already exists");
            }
        }

        // For composite key changes, we need to remove old and add new
        if ((dto.AccountNumber != null && dto.AccountNumber != accountNumber) || 
            (dto.BankName != null && dto.BankName != bankName))
        {
            _bankAccountRepository.Remove(bankAccount);
            await _bankAccountRepository.SaveChangesAsync();

            var newBankAccount = new BankAccount
            {
                AccountNumber = dto.AccountNumber ?? accountNumber,
                BankName = dto.BankName ?? bankName,
                AccountName = dto.AccountName ?? bankAccount.AccountName,
                EmployeeId = bankAccount.EmployeeId
            };

            await _bankAccountRepository.AddAsync(newBankAccount);
            await _bankAccountRepository.SaveChangesAsync();

            return MapToDto(newBankAccount);
        }

        // Update only account name if no key change
        if (dto.AccountName != null)
            bankAccount.AccountName = dto.AccountName;

        _bankAccountRepository.Update(bankAccount);
        await _bankAccountRepository.SaveChangesAsync();

        return MapToDto(bankAccount);
    }

    public async Task<bool> DeleteAsync(string accountNumber, string bankName, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByKeysAndEmployeeIdAsync(accountNumber, bankName, employeeId);
        if (bankAccount == null)
        {
            return false;
        }

        _bankAccountRepository.Remove(bankAccount);
        await _bankAccountRepository.SaveChangesAsync();

        return true;
    }

    private static BankAccountRecordDto MapToDto(BankAccount bankAccount)
    {
        return new BankAccountRecordDto
        {
            AccountNumber = bankAccount.AccountNumber,
            BankName = bankAccount.BankName,
            AccountName = bankAccount.AccountName,
            EmployeeId = bankAccount.EmployeeId
        };
    }
}
