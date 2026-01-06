using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobLevelsController : ControllerBase
{
  private readonly AppDbContext _db;

  public JobLevelsController(AppDbContext db) => _db = db;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<JobLevelDto>>> GetAll()
  {
    var jobLevels = await _db.JobLevels
        .Select(j => new JobLevelDto(j.Id, j.Name))
        .ToListAsync();
    return Ok(jobLevels);
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<JobLevelDto>> GetOne(long id)
  {
    var jobLevel = await _db.JobLevels.FindAsync(id);
    if (jobLevel is null)
      return NotFound();

    return Ok(new JobLevelDto(jobLevel.Id, jobLevel.Name));
  }
}

public record JobLevelDto(long Id, string Name);


