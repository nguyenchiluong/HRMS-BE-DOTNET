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

    public async Task<BankAccountRecordDto?> GetFirstByEmployeeIdAsync(long employeeId)
    {
        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        var bankAccount = await _bankAccountRepository.GetFirstByEmployeeIdAsync(employeeId);
        return bankAccount == null ? null : MapToDto(bankAccount);
    }

    public async Task<BankAccountRecordDto?> GetByKeysAsync(string accountNumber, string bankName, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByKeysAndEmployeeIdAsync(accountNumber, bankName, employeeId);
        return bankAccount == null ? null : MapToDto(bankAccount);
    }

    public async Task<BankAccountRecordDto?> GetByBankNameAsync(string bankName, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByBankNameAndEmployeeIdAsync(bankName, employeeId);
        return bankAccount == null ? null : MapToDto(bankAccount);
    }

    public async Task<BankAccountRecordDto> UpsertByBankNameAsync(string bankName, long employeeId, CreateBankAccountDto dto)
    {
        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        // Normalize SwiftCode to uppercase if provided
        var swiftCode = !string.IsNullOrWhiteSpace(dto.SwiftCode) ? dto.SwiftCode.ToUpperInvariant() : null;

        // Check if account with same accountNumber and bankName exists (for other employees)
        var existingByKey = await _bankAccountRepository.GetByCompositeKeyAsync(dto.AccountNumber, bankName);
        if (existingByKey != null && existingByKey.EmployeeId != employeeId)
        {
            throw new InvalidOperationException($"Bank account with number '{dto.AccountNumber}' at '{bankName}' already exists for another employee");
        }

        // Get existing account by bankName for this employee (if any)
        var existing = await _bankAccountRepository.GetByBankNameAndEmployeeIdAsync(bankName, employeeId);
        
        if (existing != null)
        {
            // Update existing account - replace all fields
            existing.AccountNumber = dto.AccountNumber;
            existing.BankName = bankName; // Use bankName from path
            existing.AccountName = dto.AccountName;
            existing.SwiftCode = swiftCode;
            existing.BranchCode = dto.BranchCode;

            _bankAccountRepository.Update(existing);
            
            // Enforce single account per employee: delete any other existing accounts
            var allAccounts = await _bankAccountRepository.GetByEmployeeIdAsync(employeeId);
            foreach (var account in allAccounts)
            {
                if (account.AccountNumber != existing.AccountNumber || account.BankName != existing.BankName)
                {
                    _bankAccountRepository.Remove(account);
                }
            }
            
            await _bankAccountRepository.SaveChangesAsync();

            return MapToDto(existing);
        }
        else
        {
            // Enforce single account per employee: delete any existing accounts
            var existingAccounts = await _bankAccountRepository.GetByEmployeeIdAsync(employeeId);
            foreach (var existingAccount in existingAccounts)
            {
                _bankAccountRepository.Remove(existingAccount);
            }
            await _bankAccountRepository.SaveChangesAsync();

            // Create new account
            var bankAccount = new BankAccount
            {
                EmployeeId = employeeId,
                AccountNumber = dto.AccountNumber,
                BankName = bankName, // Use bankName from path
                AccountName = dto.AccountName,
                SwiftCode = swiftCode,
                BranchCode = dto.BranchCode
            };

            await _bankAccountRepository.AddAsync(bankAccount);
            await _bankAccountRepository.SaveChangesAsync();

            return MapToDto(bankAccount);
        }
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

        // Normalize SwiftCode to uppercase if provided
        var swiftCode = !string.IsNullOrWhiteSpace(dto.SwiftCode) ? dto.SwiftCode.ToUpperInvariant() : null;

        var bankAccount = new BankAccount
        {
            EmployeeId = employeeId,
            AccountNumber = dto.AccountNumber,
            BankName = dto.BankName,
            AccountName = dto.AccountName,
            SwiftCode = swiftCode,
            BranchCode = dto.BranchCode
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

            // Normalize SwiftCode to uppercase if provided
            var swiftCode = dto.SwiftCode != null ? dto.SwiftCode.ToUpperInvariant() : bankAccount.SwiftCode;

            var newBankAccount = new BankAccount
            {
                AccountNumber = dto.AccountNumber ?? accountNumber,
                BankName = dto.BankName ?? bankName,
                AccountName = dto.AccountName ?? bankAccount.AccountName,
                SwiftCode = swiftCode,
                BranchCode = dto.BranchCode ?? bankAccount.BranchCode,
                EmployeeId = bankAccount.EmployeeId
            };

            await _bankAccountRepository.AddAsync(newBankAccount);
            await _bankAccountRepository.SaveChangesAsync();

            return MapToDto(newBankAccount);
        }

        // Update fields if provided
        if (dto.AccountName != null)
            bankAccount.AccountName = dto.AccountName;
        if (dto.SwiftCode != null)
            bankAccount.SwiftCode = dto.SwiftCode.ToUpperInvariant();
        if (dto.BranchCode != null)
            bankAccount.BranchCode = dto.BranchCode;

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

    public async Task<bool> DeleteByBankNameAsync(string bankName, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByBankNameAndEmployeeIdAsync(bankName, employeeId);
        if (bankAccount == null)
        {
            return false;
        }

        _bankAccountRepository.Remove(bankAccount);
        await _bankAccountRepository.SaveChangesAsync();

        return true;
    }

    public async Task<BankAccountRecordDto?> GetByIdAsync(long id, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAndEmployeeIdAsync(id, employeeId);
        return bankAccount == null ? null : MapToDto(bankAccount);
    }

    public async Task<BankAccountRecordDto?> UpdateByIdAsync(long id, long employeeId, UpdateBankAccountDto dto)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAndEmployeeIdAsync(id, employeeId);
        if (bankAccount == null)
        {
            return null;
        }

        // Check if trying to change account number or bank name to an existing account
        if (dto.AccountNumber != null || dto.BankName != null)
        {
            var newAccountNumber = dto.AccountNumber ?? bankAccount.AccountNumber;
            var newBankName = dto.BankName ?? bankAccount.BankName;
            
            var existing = await _bankAccountRepository.GetByCompositeKeyAsync(newAccountNumber, newBankName);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException($"Bank account with number '{newAccountNumber}' at '{newBankName}' already exists");
            }
        }

        // Update fields if provided
        if (dto.AccountNumber != null)
            bankAccount.AccountNumber = dto.AccountNumber;
        if (dto.BankName != null)
            bankAccount.BankName = dto.BankName;
        if (dto.AccountName != null)
            bankAccount.AccountName = dto.AccountName;
        if (dto.SwiftCode != null)
            bankAccount.SwiftCode = dto.SwiftCode.ToUpperInvariant();
        if (dto.BranchCode != null)
            bankAccount.BranchCode = dto.BranchCode;

        _bankAccountRepository.Update(bankAccount);
        await _bankAccountRepository.SaveChangesAsync();

        return MapToDto(bankAccount);
    }

    public async Task<bool> DeleteByIdAsync(long id, long employeeId)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAndEmployeeIdAsync(id, employeeId);
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
            Id = bankAccount.Id,
            AccountNumber = bankAccount.AccountNumber,
            BankName = bankAccount.BankName,
            AccountName = bankAccount.AccountName,
            SwiftCode = bankAccount.SwiftCode,
            BranchCode = bankAccount.BranchCode,
            EmployeeId = bankAccount.EmployeeId
        };
    }
}
