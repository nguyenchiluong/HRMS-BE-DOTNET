using EmployeeApi.Dtos;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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
            // TODO: Get current user's employee ID from authentication
            var currentEmployeeId = 1; // Default for now

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
            // TODO: Get current user's employee ID from authentication
            var currentEmployeeId = 1; // Default for now

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
            // TODO: Get current user's employee ID and role from authentication
            // For now, if employee_id is not provided, default to current user (employee_id = 1)
            var currentEmployeeId = employee_id ?? 1;

            var result = await _attendanceService.GetAttendanceHistoryAsync(
                currentEmployeeId,
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
