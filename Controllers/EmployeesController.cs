using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Dtos;
using EmployeeApi.Services;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;
    public EmployeesController(IEmployeeService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll([FromQuery] string? search = null) =>
        Ok(await _service.GetAllAsync(search));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<EmployeeDto>> GetOne(long id)
    {
        var dto = await _service.GetOneAsync(id);
        return dto is null ? NotFound() : Ok(dto);
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
