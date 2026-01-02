using EmployeeApi.Dtos;
using EmployeeApi.Extensions;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/time-off")]
[Authorize]
public class TimeOffController : ControllerBase
{
    private readonly ITimeOffService _timeOffService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<TimeOffController> _logger;

    public TimeOffController(
        ITimeOffService timeOffService,
        IUserContextService userContextService,
        ILogger<TimeOffController> logger)
    {
        _timeOffService = timeOffService;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a time-off request
    /// </summary>
    [HttpPost("requests")]
    public async Task<ActionResult<TimeOffRequestResponseDto>> SubmitTimeOffRequest(
        [FromBody] SubmitTimeOffRequestDto dto)
    {
        try
        {
            // Validate dates
            if (dto.StartDate > dto.EndDate)
            {
                return BadRequest(new { error = "Validation failed", message = "Start date must be before or equal to end date", field = "startDate" });
            }

            if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return BadRequest(new { error = "Validation failed", message = "Start date cannot be in the past", field = "startDate" });
            }

            // Get current user's employee ID
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            // Pass attachment URLs directly from DTO (already uploaded to S3 by frontend)
            var result = await _timeOffService.SubmitTimeOffRequestAsync(dto, currentEmployeeId, dto.Attachments);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Validation failed", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting time-off request");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get leave balances for the authenticated employee
    /// </summary>
    [HttpGet("balances")]
    public async Task<ActionResult<LeaveBalancesResponseDto>> GetLeaveBalances([FromQuery] int year = 0)
    {
        try
        {
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var currentYear = year == 0 ? DateTime.UtcNow.Year : year;

            var result = await _timeOffService.GetLeaveBalancesAsync(currentEmployeeId, currentYear);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balances");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get time-off request history for the authenticated employee
    /// </summary>
    [HttpGet("requests")]
    public async Task<ActionResult<TimeOffRequestHistoryResponseDto>> GetTimeOffRequestHistory(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null)
    {
        try
        {
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            var result = await _timeOffService.GetTimeOffRequestHistoryAsync(
                currentEmployeeId,
                page,
                limit,
                status,
                type);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time-off request history");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a time-off request
    /// </summary>
    [HttpPatch("requests/{requestId}/cancel")]
    public async Task<ActionResult<TimeOffRequestResponseDto>> CancelTimeOffRequest(
        string requestId,
        [FromBody] CancelTimeOffRequestDto? dto = null)
    {
        try
        {
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            var result = await _timeOffService.CancelTimeOffRequestAsync(requestId, currentEmployeeId, dto?.Comment);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = "Request not found", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = "Forbidden", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling time-off request {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }
}

