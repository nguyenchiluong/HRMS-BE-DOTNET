using EmployeeApi.Data;
using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly AppDbContext _context;

    public LeaveBalanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveBalance?> GetLeaveBalanceAsync(long employeeId, string balanceType, int year)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId 
                && lb.BalanceType == balanceType 
                && lb.Year == year);
    }

    public async Task<List<LeaveBalance>> GetLeaveBalancesAsync(long employeeId, int year)
    {
        return await _context.LeaveBalances
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .ToListAsync();
    }

    public async Task<LeaveBalance> CreateOrUpdateLeaveBalanceAsync(LeaveBalance leaveBalance)
    {
        var existing = await GetLeaveBalanceAsync(leaveBalance.EmployeeId, leaveBalance.BalanceType, leaveBalance.Year);
        
        if (existing != null)
        {
            existing.Total = leaveBalance.Total;
            existing.Used = leaveBalance.Used;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.LeaveBalances.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            leaveBalance.CreatedAt = DateTime.UtcNow;
            leaveBalance.UpdatedAt = DateTime.UtcNow;
            _context.LeaveBalances.Add(leaveBalance);
            await _context.SaveChangesAsync();
            return leaveBalance;
        }
    }

    public async Task<decimal> GetUsedLeaveDaysAsync(long employeeId, string balanceType, int year)
    {
        // This will be calculated from approved requests
        // For now, return 0 - will be implemented in service layer
        return 0;
    }
}

