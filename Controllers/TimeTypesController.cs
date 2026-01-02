using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeTypesController : ControllerBase
{
  private readonly AppDbContext _db;

  public TimeTypesController(AppDbContext db) => _db = db;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<TimeTypeDto>>> GetAll()
  {
    var timeTypes = await _db.TimeTypes
        .Select(t => new TimeTypeDto(t.Id, t.Name))
        .ToListAsync();
    return Ok(timeTypes);
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<TimeTypeDto>> GetOne(long id)
  {
    var timeType = await _db.TimeTypes.FindAsync(id);
    if (timeType is null)
      return NotFound();

    return Ok(new TimeTypeDto(timeType.Id, timeType.Name));
  }
}

public record TimeTypeDto(long Id, string Name);

