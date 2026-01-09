using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Dtos;
using EmployeeApi.Services.Employee;
using EmployeeApi.Services;
using EmployeeApi.Extensions;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeReadService _readService;
    private readonly IEmployeeWriteService _writeService;
    private readonly IUserContextService _userContextService;
    private readonly EmployeeAuthorizationService _authorizationService;
    private readonly IDashboardService _dashboardService;

    public EmployeesController(
        IEmployeeReadService readService,
        IEmployeeWriteService writeService,
        IUserContextService userContextService,
        EmployeeAuthorizationService authorizationService,
        IDashboardService dashboardService)
    {
        _readService = readService;
        _writeService = writeService;
        _userContextService = userContextService;
        _authorizationService = authorizationService;
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<EmployeePaginatedResponse<FilteredEmployeeDto>>> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string[]? status = null,
        [FromQuery] string[]? department = null,
        [FromQuery] string[]? position = null,
        [FromQuery] string[]? jobLevel = null,
        [FromQuery] string[]? employmentType = null,
        [FromQuery] string[]? timeType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 14)
    {
        try
        {
            // Parse arrays (from multiple query params) and comma-separated values into lists
            List<string>? statuses = ParseFilterArray(status);
            List<string>? departments = ParseFilterArray(department);
            List<string>? positions = ParseFilterArray(position);
            List<string>? jobLevels = ParseFilterArray(jobLevel);
            List<string>? employmentTypes = ParseFilterArray(employmentType);
            List<string>? timeTypes = ParseFilterArray(timeType);

            var result = await _readService.GetFilteredAsync(
                searchTerm,
                statuses,
                departments,
                positions,
                jobLevels,
                employmentTypes,
                timeTypes,
                page,
                pageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("manager/{managerId:long}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByManager(long managerId)
    {
        try
        {
            var employees = await _readService.GetByManagerIdAsync(managerId);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all employees who can serve as managers
    /// </summary>
    [HttpGet("managers")]
    public async Task<ActionResult<IEnumerable<ManagerOrHrDto>>> GetManagers([FromQuery] string? search = null)
    {
        try
        {
            var managers = await _readService.GetManagersAsync(search);
            return Ok(managers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all employees who are HR personnel
    /// </summary>
    [HttpGet("hr")]
    public async Task<ActionResult<IEnumerable<ManagerOrHrDto>>> GetHrPersonnel([FromQuery] string? search = null)
    {
        try
        {
            var hrPersonnel = await _readService.GetHrPersonnelAsync(search);
            return Ok(hrPersonnel);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Parses an array of strings (from multiple query params or comma-separated values) into a list
    /// Supports both formats: ?department=Product&department=Engineering OR ?department=Product,Engineering
    /// </summary>
    private static List<string>? ParseFilterArray(string[]? values)
    {
        if (values == null || values.Length == 0)
            return null;

        // Flatten: split each value by comma (in case of comma-separated) and combine all
        var items = values
            .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        return items.Count > 0 ? items : null;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<EmployeeStatsDto>> GetStats()
    {
        try
        {
            var stats = await _readService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<EmployeeDto>> GetOne(long id)
    {
        var dto = await _readService.GetOneAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Gets the current authenticated employee's information from JWT token
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> GetCurrentEmployee()
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var dto = await _readService.GetOneAsync(employeeId.Value);

            if (dto == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets dashboard statistics for the current authenticated employee
    /// </summary>
    /// <remarks>
    /// Returns aggregated statistics including:
    /// - bonusBalance: Current bonus credit points balance (placeholder, actual value from Spring Boot)
    /// - pendingTimesheets: Count of timesheets with status "PENDING"
    /// - totalHoursThisMonth: Sum of approved timesheet hours for current month
    /// - leaveBalance: Remaining leave days for the year
    /// </remarks>
    /// <response code="200">Successfully retrieved dashboard statistics</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me/dashboard-stats")]
    [ProducesResponseType(typeof(EmployeeDashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDashboardStatsDto>> GetCurrentEmployeeDashboardStats()
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var stats = await _dashboardService.GetEmployeeDashboardStatsAsync(employeeId.Value);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the current authenticated employee's profile information
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/employees/me
    ///     Authorization: Bearer {your-jwt-token}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "firstName": "John",
    ///       "lastName": "Doe",
    ///       "phone": "+1234567890",
    ///       "personalEmail": "john.doe@personal.com",
    ///       "currentAddress": "123 Main St, City, Country"
    ///     }
    ///
    /// Allows employees to update their personal information. Only the fields provided will be updated.
    /// </remarks>
    /// <response code="200">Successfully updated the employee profile</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Employee ID not found in JWT token or authentication failed</response>
    /// <response code="404">Employee record not found in the database</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmployeeDto>> UpdateCurrentEmployeeProfile([FromBody] UpdateProfileDto input)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var dto = await _writeService.UpdateProfileAsync(employeeId.Value, input);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Employee not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Validates onboarding token and returns employee info for the onboarding form (including education and bank account)
    /// Returns OnboardDto so frontend can directly use it to populate and submit the form
    /// </summary>
    [HttpGet("onboarding-info")]
    public async Task<ActionResult<OnboardDto>> GetOnboardingInfo([FromQuery] string token)
    {
        try
        {
            var dto = await _readService.GetByOnboardingTokenAsync(token);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Employee not found" });
        }
    }

    /// <summary>
    /// Saves onboarding progress without completing it
    /// </summary>
    [HttpPut("onboarding-progress")]
    public async Task<ActionResult<EmployeeDto>> SaveOnboardingProgress(
        [FromQuery] string token,
        [FromBody] OnboardDto input)
    {
        try
        {
            var dto = await _writeService.SaveOnboardingProgressAsync(token, input);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Employee not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto input)
    {
        try
        {
            var dto = await _writeService.CreateAsync(input);
            return CreatedAtAction(nameof(GetOne), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Admin creates initial profile for a new hire with basic employment info
    /// </summary>
    [HttpPost("initial-profile")]
    public async Task<ActionResult<EmployeeDto>> CreateInitialProfile([FromBody] InitialProfileDto input)
    {
        try
        {
            var dto = await _writeService.CreateInitialProfileAsync(input);
            return CreatedAtAction(nameof(GetOne), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// New hire completes their onboarding with personal details and education
    /// </summary>
    [HttpPost("{id:long}/onboard")]
    public async Task<ActionResult<EmployeeDto>> CompleteOnboarding(long id, [FromBody] OnboardDto input)
    {
        try
        {
            var dto = await _writeService.CompleteOnboardingAsync(id, input);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Employee not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reassigns supervisors (manager and HR) for an employee
    /// Only the HR (admin) who manages this employee can reassign supervisors
    /// </summary>
    /// <param name="employeeId">The ID of the employee to update</param>
    /// <param name="input">The supervisor assignment data (both managerId and hrId are required, null means remove assignment)</param>
    /// <returns>Success message</returns>
    /// <response code="200">Supervisors updated successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="403">Forbidden - Only the HR manager of this employee can reassign supervisors</response>
    /// <response code="404">Employee not found</response>
    [HttpPut("{employeeId:long}/supervisors")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReassignSupervisors(long employeeId, [FromBody] ReassignSupervisorsDto input)
    {
        try
        {
            // Get current user's employee ID
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);

            // Check authorization
            var authResult = await _authorizationService.CanReassignSupervisorsAsync(currentEmployeeId, employeeId);
            if (!authResult.IsAuthorized)
            {
                // Check if it's a "not found" error (404) or authorization error (403)
                if (authResult.ErrorMessage == "Employee not found")
                {
                    return NotFound(new { message = authResult.ErrorMessage });
                }
                return StatusCode(403, new { message = authResult.ErrorMessage });
            }

            await _writeService.ReassignSupervisorsAsync(employeeId, input);
            return Ok(new { message = "Supervisors updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Employee not found" });
        }
        catch (ArgumentException ex)
        {
            // Parse validation errors from exception message
            var errorMessage = ex.Message;
            var errors = new List<string>();

            if (errorMessage.Contains("Validation failed:"))
            {
                var errorPart = errorMessage.Substring(errorMessage.IndexOf(":") + 1).Trim();
                errors = errorPart.Split(',').Select(e => e.Trim()).ToList();
            }
            else
            {
                errors.Add(errorMessage);
            }

            return BadRequest(new { message = "Validation failed", errors });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Resends the onboarding email link for an employee with pending onboarding status
    /// </summary>
    /// <remarks>
    /// This endpoint allows admins to resend the onboarding form link to employees who still have "pending onboarding" status.
    /// A new token will be generated (effectively invalidating any previous token) and sent via email to the employee's personal email.
    /// 
    /// Note: The old token will still work until it expires based on its original timestamp, but the new token will be sent to the employee.
    /// </remarks>
    /// <param name="employeeId">The ID of the employee to resend the onboarding email for</param>
    /// <returns>Success message</returns>
    /// <response code="200">Onboarding email resent successfully</response>
    /// <response code="400">Employee does not have pending onboarding status or personal email is missing</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Employee not found</response>
    [HttpPost("{employeeId:long}/resend-onboarding-email")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResendOnboardingEmail(long employeeId)
    {
        try
        {
            await _writeService.ResendOnboardingEmailAsync(employeeId);
            return Ok(new { message = "Onboarding email resent successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Employee not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
