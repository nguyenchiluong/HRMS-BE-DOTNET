using EmployeeApi.Dtos;
using EmployeeApi.Extensions;
using EmployeeApi.Repositories;
using EmployeeApi.Services;
using EmployeeApi.Services.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;
    private readonly IUserContextService _userContextService;
    private readonly IRequestRepository _requestRepository;
    private readonly IEmployeeReadService _employeeReadService;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(
        IRequestService requestService,
        IUserContextService userContextService,
        IRequestRepository requestRepository,
        IEmployeeReadService employeeReadService,
        ILogger<RequestsController> logger)
    {
        _requestService = requestService;
        _userContextService = userContextService;
        _requestRepository = requestRepository;
        _employeeReadService = employeeReadService;
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
        [FromQuery] string? category = null,
        [FromQuery] long? employee_id = null,
        [FromQuery] DateTime? date_from = null,
        [FromQuery] DateTime? date_to = null)
    {
        try
        {
            // Get current user's employee ID from JWT token (maps email to employee_id)
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var userRole = _userContextService.GetRoleFromClaims(User);

            // Check if user is a manager/admin by role OR by having direct reports in database
            var isManagerOrAdminByRole = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
                                      || userRole.Equals("MANAGER", StringComparison.OrdinalIgnoreCase);

            // Also check if user has direct reports in the database (even if role claim is wrong)
            var directReportIds = await _requestRepository.GetDirectReportEmployeeIdsAsync(currentEmployeeId);
            var hasDirectReports = directReportIds.Count > 0;

            var isManagerOrAdmin = isManagerOrAdminByRole || hasDirectReports;

            // Normalize category (lowercase)
            string? categoryFilter = null;
            if (!string.IsNullOrEmpty(category))
            {
                categoryFilter = category.ToLower().Trim();
            }

            // Determine if we need manager filtering for approval request categories
            // Categories that require approval: "timesheet" and "time-off"
            bool filterByManagerReports = false;
            long? managerId = null;

            if (isManagerOrAdmin && !string.IsNullOrEmpty(categoryFilter))
            {
                var approvalCategories = new HashSet<string> { "timesheet", "time-off" };

                if (approvalCategories.Contains(categoryFilter))
                {
                    // For approval requests, managers should only see their direct reports
                    filterByManagerReports = true;
                    managerId = currentEmployeeId;
                    // Don't filter by employee_id for approval requests when manager filtering is enabled
                    employee_id = null;
                }
            }

            // Check if user is admin for profile requests
            // Only admins can view profile change requests, and they only see requests assigned to them as approver
            bool filterByApprover = false;
            long? approverId = null;

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter == "profile")
            {
                // Check if user is admin (only admins can view profile requests)
                var isAdminByRole = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                var isAdminInDb = await _employeeReadService.IsAdminAsync(currentEmployeeId);
                var isAdmin = isAdminByRole || isAdminInDb;

                if (!isAdmin)
                {
                    return StatusCode(403, new { error = "Forbidden", message = "Only admins can view profile change requests" });
                }

                // Admin can only see profile requests where they are assigned as approver
                filterByApprover = true;
                approverId = currentEmployeeId;
                employee_id = null; // Don't filter by employee_id when filtering by approver
            }

            // Regular employees see only their own requests
            // When filterByManagerReports or filterByApprover is true, set filterEmployeeId to null so filtering can work
            long? filterEmployeeId = (filterByManagerReports || filterByApprover)
                ? null
                : (isManagerOrAdmin ? employee_id : currentEmployeeId);

            // Log for debugging
            _logger.LogInformation(
                "GetRequests - ManagerId: {ManagerId}, EmployeeId: {EmployeeId}, Role: {Role}, Category: {Category}, FilterByManagerReports: {FilterByManagerReports}, ApproverId: {ApproverId}, FilterByApprover: {FilterByApprover}, FilterEmployeeId: {FilterEmployeeId}",
                managerId, currentEmployeeId, userRole,
                categoryFilter ?? "null",
                filterByManagerReports,
                approverId?.ToString() ?? "null",
                filterByApprover,
                filterEmployeeId);

            var result = await _requestService.GetRequestsAsync(
                filterEmployeeId,
                status?.ToUpper(),
                categoryFilter,
                date_from,
                date_to,
                page,
                limit,
                managerId,
                filterByManagerReports,
                approverId,
                filterByApprover);

            // Log result count for debugging
            _logger.LogInformation(
                "GetRequests - Result: {Count} requests found",
                result.Data.Count);

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
                var details = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = string.Join("; ", ms.Value!.Errors.Select(e => e.ErrorMessage)) })
                    .ToList();

                return BadRequest(new
                {
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "Request validation failed",
                        details = details
                    }
                });
            }

            // Get current user's employee ID from JWT token (maps email to employee_id)
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            // Get user role from JWT token to pass to service layer
            var userRole = _userContextService.GetRoleFromClaims(User);

            // Create request - service layer will handle auto-approval logic
            var request = await _requestService.CreateRequestAsync(dto, currentEmployeeId, userRole);

            return CreatedAtAction(
                nameof(GetRequest),
                new { id = request.Id },
                new { message = "Request created successfully", data = request });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating request");
            // Parse validation error message to extract field and message
            var errorParts = ex.Message.Split(new string[] { ": " }, 2, StringSplitOptions.None);
            var errorMessage = errorParts.Length > 1 ? errorParts[1] : ex.Message;

            return BadRequest(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = errorMessage
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating request");
            return StatusCode(500, new { error = new { code = "INTERNAL_SERVER_ERROR", message = ex.Message } });
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

            // Get current user's employee ID from JWT token (maps email to employee_id)
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

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
    public async Task<ActionResult<object>> CancelRequest(int id, [FromBody] CancelRequestDto? dto = null)
    {
        try
        {
            // Get current user's employee ID from JWT token (maps email to employee_id)
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            var result = await _requestService.CancelRequestAsync(id, currentEmployeeId, dto?.Comment);

            return Ok(new { message = "Request cancelled successfully", data = new { id = id.ToString(), status = "CANCELLED", updatedAt = DateTime.UtcNow.ToString("O") } });
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
    /// Approve request (Manager/Admin only, but only Admin for profile requests)
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<object>> ApproveRequest(int id, [FromBody] ApprovalDto? dto)
    {
        try
        {
            // Get current user's employee ID from JWT token and verify role
            var userRole = _userContextService.GetRoleFromClaims(User);
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            // First, get the request from repository to check its category
            var request = await _requestRepository.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { error = "Not Found", message = "Request not found" });
            }

            // Check if this is a profile request by checking the request type category
            var isProfileRequest = request.RequestTypeLookup?.Category?.ToLower() == "profile" ||
                (request.RequestTypeLookup?.Code != null && request.RequestTypeLookup.Code.Contains("PROFILE", StringComparison.OrdinalIgnoreCase));

            // For profile requests, only admins can approve
            if (isProfileRequest)
            {
                var isAdminByRole = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                var isAdminInDb = await _employeeReadService.IsAdminAsync(currentEmployeeId);
                var isAdmin = isAdminByRole || isAdminInDb;

                if (!isAdmin)
                {
                    return StatusCode(403, new { error = "Forbidden", message = "Only admins can approve profile change requests" });
                }
            }
            else
            {
                // For other requests, managers or admins can approve
                var isManagerOrAdmin = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
                                    || userRole.Equals("MANAGER", StringComparison.OrdinalIgnoreCase);

                if (!isManagerOrAdmin)
                {
                    return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can approve requests" });
                }
            }

            var approvedRequest = await _requestService.ApproveRequestAsync(id, currentEmployeeId, dto?.Comment);

            return Ok(new { message = "Request approved successfully", data = approvedRequest });
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
    /// Reject request (Manager/Admin only, but only Admin for profile requests)
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
            var userRole = _userContextService.GetRoleFromClaims(User);
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            // First, get the request from repository to check its category
            var request = await _requestRepository.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { error = "Not Found", message = "Request not found" });
            }

            // Check if this is a profile request by checking the request type category
            var isProfileRequest = request.RequestTypeLookup?.Category?.ToLower() == "profile" ||
                (request.RequestTypeLookup?.Code != null && request.RequestTypeLookup.Code.Contains("PROFILE", StringComparison.OrdinalIgnoreCase));

            // For profile requests, only admins can reject
            if (isProfileRequest)
            {
                var isAdminByRole = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                var isAdminInDb = await _employeeReadService.IsAdminAsync(currentEmployeeId);
                var isAdmin = isAdminByRole || isAdminInDb;

                if (!isAdmin)
                {
                    return StatusCode(403, new { error = "Forbidden", message = "Only admins can reject profile change requests" });
                }
            }
            else
            {
                // For other requests, managers or admins can reject
                var isManagerOrAdmin = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
                                    || userRole.Equals("MANAGER", StringComparison.OrdinalIgnoreCase);

                if (!isManagerOrAdmin)
                {
                    return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can reject requests" });
                }
            }

            var rejectedRequest = await _requestService.RejectRequestAsync(id, currentEmployeeId, dto.Reason);

            return Ok(new { message = "Request rejected successfully", data = rejectedRequest });
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
