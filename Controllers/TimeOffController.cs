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
    private readonly IWebHostEnvironment _environment;

    public TimeOffController(
        ITimeOffService timeOffService,
        IUserContextService userContextService,
        ILogger<TimeOffController> logger,
        IWebHostEnvironment environment)
    {
        _timeOffService = timeOffService;
        _userContextService = userContextService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Submit a time-off request
    /// </summary>
    [HttpPost("requests")]
    public async Task<ActionResult<TimeOffRequestResponseDto>> SubmitTimeOffRequest(
        [FromForm] string type,
        [FromForm] string startDate,
        [FromForm] string endDate,
        [FromForm] string reason,
        [FromForm] List<IFormFile>? attachments = null)
    {
        try
        {
            // Parse dates
            if (!DateOnly.TryParse(startDate, out var startDateParsed))
            {
                return BadRequest(new { error = "Validation failed", message = "Invalid start date format. Use ISO format: yyyy-MM-dd", field = "startDate" });
            }

            if (!DateOnly.TryParse(endDate, out var endDateParsed))
            {
                return BadRequest(new { error = "Validation failed", message = "Invalid end date format. Use ISO format: yyyy-MM-dd", field = "endDate" });
            }

            // Validate dates
            if (startDateParsed > endDateParsed)
            {
                return BadRequest(new { error = "Validation failed", message = "Start date must be before or equal to end date", field = "startDate" });
            }

            if (startDateParsed < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return BadRequest(new { error = "Validation failed", message = "Start date cannot be in the past", field = "startDate" });
            }

            // Create DTO
            var dto = new SubmitTimeOffRequestDto
            {
                Type = type,
                StartDate = startDateParsed,
                EndDate = endDateParsed,
                Reason = reason
            };

            // Get current user's employee ID
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            // Handle file uploads
            List<string>? attachmentUrls = null;
            if (attachments != null && attachments.Count > 0)
            {
                // Validate file count
                if (attachments.Count > 5)
                {
                    return BadRequest(new { error = "Validation failed", message = "Maximum 5 files allowed per request" });
                }

                attachmentUrls = new List<string>();
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "time-off-requests");
                Directory.CreateDirectory(uploadsPath);

                foreach (var file in attachments)
                {
                    // Validate file size (10MB max)
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { error = "Validation failed", message = $"File {file.FileName} exceeds maximum size of 10MB" });
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { error = "Validation failed", message = $"File type {extension} is not allowed. Allowed types: PDF, JPG, JPEG, PNG, DOC, DOCX" });
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Store relative URL (in production, this should be a full URL)
                    attachmentUrls.Add($"/uploads/time-off-requests/{fileName}");
                }
            }

            var result = await _timeOffService.SubmitTimeOffRequestAsync(dto, currentEmployeeId, attachmentUrls);

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

