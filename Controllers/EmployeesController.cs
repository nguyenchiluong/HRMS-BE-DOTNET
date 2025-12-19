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

}
