using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Dtos;
using EmployeeApi.Services;

namespace EmployeeApi.Controllers;

/// <summary>
/// Controller for admin-specific endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public AdminController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets dashboard statistics for the admin dashboard
    /// </summary>
    /// <remarks>
    /// Returns aggregated statistics including:
    /// - totalEmployees: Total count of all employees
    /// - activeEmployees: Count of employees with status "active"
    /// - departments: Count of distinct departments
    /// - totalPayroll: Sum of Position.Salary for all employees with positions
    /// - pendingApprovals.timesheets: Count of pending timesheet requests
    /// - pendingApprovals.timeOff: Count of pending time-off requests
    /// 
    /// Note: Campaign stats are served by the Spring Boot service at GET /api/campaigns/stats
    /// The frontend should call both endpoints and merge the results.
    /// </remarks>
    /// <response code="200">Successfully retrieved admin dashboard statistics</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("dashboard-stats")]
    [ProducesResponseType(typeof(AdminDashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetAdminDashboardStats()
    {
        try
        {
            var stats = await _dashboardService.GetAdminDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
