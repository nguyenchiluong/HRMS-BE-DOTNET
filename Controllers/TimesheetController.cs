using EmployeeApi.Dtos;
using EmployeeApi.Services;
using EmployeeApi.Services.Timesheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TimesheetController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<TimesheetController> _logger;

    public TimesheetController(
        ITimesheetService timesheetService,
        IUserContextService userContextService,
        ILogger<TimesheetController> logger)
    {
        _timesheetService = timesheetService;
        _userContextService = userContextService;
        _logger = logger;
    }

    // ========================================
    // Timesheet Submission
    // ========================================

    /// <summary>
    /// Submit a weekly timesheet
    /// POST /api/v1/timesheet/submit
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<object>> SubmitTimesheet([FromBody] SubmitTimesheetRequest dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Invalid input data", details = ModelState });
            }

            var employeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            
            // Get user role from JWT token to pass to service layer for auto-approval logic
            var userRole = _userContextService.GetRoleFromClaims(User);
            
            var result = await _timesheetService.SubmitTimesheetAsync(dto, employeeId, userRole);

            return CreatedAtAction(
                nameof(GetTimesheet),
                new { requestId = result.RequestId },
                new { message = "Timesheet submitted successfully", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting timesheet");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Adjust/update a timesheet (only for pending or rejected)
    /// PUT /api/v1/timesheet/{requestId}/adjust
    /// </summary>
    [HttpPut("{requestId}/adjust")]
    public async Task<ActionResult<object>> AdjustTimesheet(int requestId, [FromBody] AdjustTimesheetRequest dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Invalid input data", details = ModelState });
            }

            var employeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _timesheetService.AdjustTimesheetAsync(requestId, dto, employeeId);

            return Ok(new { message = "Timesheet adjusted successfully", data = result });
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
            _logger.LogError(ex, "Error adjusting timesheet {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    // ========================================
    // Timesheet Queries
    // ========================================

    /// <summary>
    /// Get a timesheet by request ID
    /// GET /api/v1/timesheet/{requestId}
    /// </summary>
    [HttpGet("{requestId}")]
    public async Task<ActionResult<TimesheetResponse>> GetTimesheet(int requestId)
    {
        try
        {
            var result = await _timesheetService.GetTimesheetByIdAsync(requestId);
            if (result == null)
            {
                return NotFound(new { error = "Not Found", message = "Timesheet not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timesheet {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get my timesheets (for current employee)
    /// GET /api/v1/timesheet/my-timesheets
    /// </summary>
    [HttpGet("my-timesheets")]
    public async Task<ActionResult<PaginatedResponseDto<TimesheetListItem>>> GetMyTimesheets(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            var employeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _timesheetService.GetMyTimesheetsAsync(
                employeeId, year, month, status?.ToUpper(), page, limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my timesheets");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    // ========================================
    // Approval Workflow
    // ========================================

    /// <summary>
    /// Get pending timesheet approvals (for manager view)
    /// GET /api/v1/timesheet/pending-approvals
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<ActionResult<PaginatedResponseDto<TimesheetApprovalItem>>> GetPendingApprovals(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            // Verify user is manager or admin
            var userRole = _userContextService.GetRoleFromClaims(User);
            var isManagerOrAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                                || userRole.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            if (!isManagerOrAdmin)
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can view pending approvals" });
            }

            var employeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _timesheetService.GetPendingApprovalsAsync(employeeId, page, limit);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Approve a timesheet
    /// PUT /api/v1/timesheet/{requestId}/approve
    /// </summary>
    [HttpPut("{requestId}/approve")]
    public async Task<ActionResult<object>> ApproveTimesheet(int requestId, [FromBody] ApprovalDto? dto)
    {
        try
        {
            // Verify user is manager or admin
            var userRole = _userContextService.GetRoleFromClaims(User);
            var isManagerOrAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                                || userRole.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            if (!isManagerOrAdmin)
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can approve timesheets" });
            }

            var approverId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _timesheetService.ApproveTimesheetAsync(requestId, approverId, dto?.Comment);

            return Ok(new { message = "Timesheet approved successfully", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving timesheet {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Reject a timesheet
    /// PUT /api/v1/timesheet/{requestId}/reject
    /// </summary>
    [HttpPut("{requestId}/reject")]
    public async Task<ActionResult<object>> RejectTimesheet(int requestId, [FromBody] RejectionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Rejection reason is required and must be at least 10 characters", details = ModelState });
            }

            // Verify user is manager or admin
            var userRole = _userContextService.GetRoleFromClaims(User);
            var isManagerOrAdmin = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                                || userRole.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            if (!isManagerOrAdmin)
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only managers or admins can reject timesheets" });
            }

            var approverId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _timesheetService.RejectTimesheetAsync(requestId, approverId, dto.Reason);

            return Ok(new { message = "Timesheet rejected successfully", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting timesheet {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a timesheet (only for pending timesheets)
    /// PUT /api/v1/timesheet/{requestId}/cancel
    /// </summary>
    [HttpPut("{requestId}/cancel")]
    public async Task<ActionResult<object>> CancelTimesheet(int requestId)
    {
        try
        {
            var employeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var result = await _timesheetService.CancelTimesheetAsync(requestId, employeeId);

            return Ok(new { message = "Timesheet cancelled successfully", data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = "Forbidden", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Check if it's a "not found" error or "not pending" error
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = "Not Found", message = ex.Message });
            }
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling timesheet {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    // ========================================
    // Task Management
    // ========================================

    /// <summary>
    /// Get all active tasks
    /// GET /api/v1/timesheet/tasks
    /// </summary>
    [HttpGet("tasks")]
    public async Task<ActionResult<List<TimesheetTaskResponse>>> GetActiveTasks()
    {
        try
        {
            var result = await _timesheetService.GetActiveTasksAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active tasks");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get all tasks (including inactive) - Admin only
    /// GET /api/v1/timesheet/tasks/all
    /// </summary>
    [HttpGet("tasks/all")]
    public async Task<ActionResult<List<TimesheetTaskResponse>>> GetAllTasks()
    {
        try
        {
            var userRole = _userContextService.GetRoleFromClaims(User);
            if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only admins can view all tasks" });
            }

            var result = await _timesheetService.GetAllTasksAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tasks");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new task - Admin only
    /// POST /api/v1/timesheet/tasks
    /// </summary>
    [HttpPost("tasks")]
    public async Task<ActionResult<object>> CreateTask([FromBody] CreateTimesheetTaskRequest dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Invalid input data", details = ModelState });
            }

            var userRole = _userContextService.GetRoleFromClaims(User);
            if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only admins can create tasks" });
            }

            var result = await _timesheetService.CreateTaskAsync(dto);

            return CreatedAtAction(
                nameof(GetActiveTasks),
                new { message = "Task created successfully", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Update a task - Admin only
    /// PUT /api/v1/timesheet/tasks/{id}
    /// </summary>
    [HttpPut("tasks/{id}")]
    public async Task<ActionResult<object>> UpdateTask(int id, [FromBody] UpdateTimesheetTaskRequest dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Bad Request", message = "Invalid input data", details = ModelState });
            }

            var userRole = _userContextService.GetRoleFromClaims(User);
            if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { error = "Forbidden", message = "Only admins can update tasks" });
            }

            var result = await _timesheetService.UpdateTaskAsync(id, dto);

            return Ok(new { message = "Task updated successfully", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Bad Request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }
}

