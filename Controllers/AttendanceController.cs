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
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Check-in for the day
    /// </summary>
    [HttpPost("check-in")]
    public async Task<ActionResult<CheckInResponseDto>> CheckIn([FromBody] CheckInDto? dto)
    {
        try
        {
            // Get current user's employee ID from JWT token
            var currentEmployeeId = User.GetEmployeeId();

            var result = await _attendanceService.CheckInAsync(currentEmployeeId, dto?.Location);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-in");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Check-out for the day
    /// </summary>
    [HttpPost("check-out")]
    public async Task<ActionResult<CheckOutResponseDto>> CheckOut()
    {
        try
        {
            // Get current user's employee ID from JWT token
            var currentEmployeeId = User.GetEmployeeId();

            var result = await _attendanceService.CheckOutAsync(currentEmployeeId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-out");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get attendance history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<PaginatedResponseDto<AttendanceRecordDto>>> GetAttendanceHistory(
        [FromQuery] int? employee_id = null,
        [FromQuery] DateTime? date_from = null,
        [FromQuery] DateTime? date_to = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            // Get current user's employee ID and role from JWT token
            var currentEmployeeId = User.GetEmployeeId();
            
            // Managers/Admins can filter by employee_id, regular employees see only their own
            var filterEmployeeId = User.IsManagerOrAdmin() ? employee_id : currentEmployeeId;

            var result = await _attendanceService.GetAttendanceHistoryAsync(
                filterEmployeeId,
                date_from,
                date_to,
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
