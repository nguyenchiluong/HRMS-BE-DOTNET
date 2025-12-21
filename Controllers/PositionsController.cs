using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
  private readonly AppDbContext _db;

  public PositionsController(AppDbContext db) => _db = db;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll()
  {
    var positions = await _db.Positions
        .Select(p => new PositionDto(p.Id, p.Title))
        .ToListAsync();
    return Ok(positions);
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<PositionDto>> GetOne(long id)
  {
    var position = await _db.Positions.FindAsync(id);
    if (position is null)
      return NotFound();

    return Ok(new PositionDto(position.Id, position.Title));
  }
}

public record PositionDto(long Id, string Title);
