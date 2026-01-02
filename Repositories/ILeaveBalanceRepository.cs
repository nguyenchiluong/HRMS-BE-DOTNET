using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetLeaveBalanceAsync(long employeeId, string balanceType, int year);
    Task<List<LeaveBalance>> GetLeaveBalancesAsync(long employeeId, int year);
    Task<LeaveBalance> CreateOrUpdateLeaveBalanceAsync(LeaveBalance leaveBalance);
    Task<decimal> GetUsedLeaveDaysAsync(long employeeId, string balanceType, int year);
}

