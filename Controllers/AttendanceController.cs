using EmployeeApi.Dtos;
using EmployeeApi.Extensions;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(
        IAttendanceService attendanceService,
        IUserContextService userContextService,
        ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// Get current clock status for today
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<CurrentClockStatusResponseDto>> GetCurrentClockStatus()
    {
        try
        {
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _attendanceService.GetCurrentClockStatusAsync(currentEmployeeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current clock status");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Clock in for the day
    /// </summary>
    [HttpPost("clock-in")]
    public async Task<ActionResult<ClockInResponseDto>> ClockIn()
    {
        try
        {
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _attendanceService.ClockInAsync(currentEmployeeId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during clock-in");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Clock out for the day
    /// </summary>
    [HttpPost("clock-out")]
    public async Task<ActionResult<ClockOutResponseDto>> ClockOut()
    {
        try
        {
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _attendanceService.ClockOutAsync(currentEmployeeId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during clock-out");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get attendance history for employee
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<AttendanceHistoryResponseDto>> GetAttendanceHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 7)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(new { error = "Bad Request", message = "Page must be greater than 0" });
            }
            if (limit < 1)
            {
                return BadRequest(new { error = "Bad Request", message = "Limit must be greater than 0" });
            }

            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _attendanceService.GetAttendanceHistoryForEmployeeAsync(
                currentEmployeeId,
                startDate,
                endDate,
                page,
                limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance history");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }
}
