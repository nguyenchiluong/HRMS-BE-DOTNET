using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Dtos;
using EmployeeApi.Services.Employee;
using EmployeeApi.Extensions;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;
    public EmployeesController(IEmployeeService service) => _service = service;

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

            var result = await _service.GetFilteredAsync(
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
            var stats = await _service.GetStatsAsync();
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
        var dto = await _service.GetOneAsync(id);
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

            var dto = await _service.GetOneAsync(employeeId.Value);
            
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

            var dto = await _service.UpdateProfileAsync(employeeId.Value, input);
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
    /// Validates onboarding token and returns employee info for the onboarding form
    /// </summary>
    [HttpGet("onboarding-info")]
    public async Task<ActionResult<EmployeeDto>> GetOnboardingInfo([FromQuery] string token)
    {
        try
        {
            var dto = await _service.GetByOnboardingTokenAsync(token);
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
            var dto = await _service.SaveOnboardingProgressAsync(token, input);
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
            var dto = await _service.CreateAsync(input);
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
            var dto = await _service.CreateInitialProfileAsync(input);
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
            var dto = await _service.CompleteOnboardingAsync(id, input);
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
}
