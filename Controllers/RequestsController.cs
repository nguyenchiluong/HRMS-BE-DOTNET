using EmployeeApi.Dtos;
using EmployeeApi.Extensions;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(IRequestService requestService, ILogger<RequestsController> logger)
    {
        _requestService = requestService;
        _logger = logger;
    }

    /// <summary>
    /// Get requests list
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseDto<RequestDto>>> GetRequests(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? request_type = null,
        [FromQuery] int? employee_id = null,
        [FromQuery] DateTime? date_from = null,
        [FromQuery] DateTime? date_to = null)
    {
        try
        {
            // Get current user's employee ID from JWT token
            var currentEmployeeId = User.GetEmployeeId();
            
            // Managers/Admins can filter by employee_id, regular employees see only their own
            var filterEmployeeId = User.IsManagerOrAdmin() ? employee_id : currentEmployeeId;

            var result = await _requestService.GetRequestsAsync(
                filterEmployeeId,
                status?.ToUpper(),
                request_type?.ToUpper(),
                date_from,
                date_to,
                page,
                limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting requests");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get request details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RequestDetailsDto>> GetRequest(int id)
    {
        try
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { error = "Not Found", message = "Request not found" });
            }

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request {RequestId}", id);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Create new request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateRequest([FromBody] CreateRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Invalid input data", details = ModelState });
            }

            // Get current user's employee ID from JWT token
            var currentEmployeeId = User.GetEmployeeId();

            var request = await _requestService.CreateRequestAsync(dto, currentEmployeeId);

            return CreatedAtAction(
                nameof(GetRequest),
                new { id = request.Id },
                new { message = "Request created successfully", data = request });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating request");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Update request
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<object>> UpdateRequest(int id, [FromBody] UpdateRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Invalid input data", details = ModelState });
            }

            // Get current user's employee ID from JWT token
            var currentEmployeeId = User.GetEmployeeId();

            var request = await _requestService.UpdateRequestAsync(id, dto, currentEmployeeId);

            return Ok(new { message = "Request updated successfully", data = request });
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
            _logger.LogError(ex, "Error updating request {RequestId}", id);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel request
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<object>> CancelRequest(int id)
    {
        try
        {
            // Get current user's employee ID from JWT token
            var currentEmployeeId = User.GetEmployeeId();

            var result = await _requestService.CancelRequestAsync(id, currentEmployeeId);

            return Ok(new { message = "Request cancelled successfully" });
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
            _logger.LogError(ex, "Error cancelling request {RequestId}", id);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Approve request (Manager/Admin only)
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<object>> ApproveRequest(int id, [FromBody] ApprovalDto? dto)
    {
        try
        {
            // Get current user's employee ID from JWT token and verify role
            if (!User.IsManagerOrAdmin())
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can approve requests" });
            }
            
            var currentEmployeeId = User.GetEmployeeId();

            var request = await _requestService.ApproveRequestAsync(id, currentEmployeeId, dto?.Comment);

            return Ok(new { message = "Request approved successfully", data = request });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving request {RequestId}", id);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Reject request (Manager/Admin only)
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<object>> RejectRequest(int id, [FromBody] RejectionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Rejection reason is required and must be at least 10 characters", details = ModelState });
            }

            // Get current user's employee ID from JWT token and verify role
            if (!User.IsManagerOrAdmin())
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can reject requests" });
            }
            
            var currentEmployeeId = User.GetEmployeeId();

            var request = await _requestService.RejectRequestAsync(id, currentEmployeeId, dto.Reason);

            return Ok(new { message = "Request rejected successfully", data = request });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting request {RequestId}", id);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get requests summary (Manager/Admin dashboard)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<RequestsSummaryDto>> GetRequestsSummary(
        [FromQuery] int? employee_id = null,
        [FromQuery] string? month = null,
        [FromQuery] string? request_type = null)
    {
        try
        {
            var summary = await _requestService.GetRequestsSummaryAsync(
                employee_id,
                month,
                request_type?.ToUpper());

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting requests summary");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }
}
