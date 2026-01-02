using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmploymentTypesController : ControllerBase
{
  private readonly AppDbContext _db;

  public EmploymentTypesController(AppDbContext db) => _db = db;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<EmploymentTypeDto>>> GetAll()
  {
    var employmentTypes = await _db.EmploymentTypes
        .Select(e => new EmploymentTypeDto(e.Id, e.Name))
        .ToListAsync();
    return Ok(employmentTypes);
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<EmploymentTypeDto>> GetOne(long id)
  {
    var employmentType = await _db.EmploymentTypes.FindAsync(id);
    if (employmentType is null)
      return NotFound();

    return Ok(new EmploymentTypeDto(employmentType.Id, employmentType.Name));
  }
}

public record EmploymentTypeDto(long Id, string Name);

