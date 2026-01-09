using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

/// <summary>
/// Service for dashboard statistics
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get employee dashboard statistics for a specific employee
    /// </summary>
    Task<EmployeeDashboardStatsDto> GetEmployeeDashboardStatsAsync(long employeeId);

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync();
}
