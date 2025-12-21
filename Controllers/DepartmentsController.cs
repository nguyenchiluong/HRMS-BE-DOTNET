using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
  private readonly AppDbContext _db;

  public DepartmentsController(AppDbContext db) => _db = db;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
  {
    var departments = await _db.Departments
        .Select(d => new DepartmentDto(d.Id, d.Name))
        .ToListAsync();
    return Ok(departments);
  }

  [HttpGet("{id:long}")]
  public async Task<ActionResult<DepartmentDto>> GetOne(long id)
  {
    var department = await _db.Departments.FindAsync(id);
    if (department is null)
      return NotFound();

    return Ok(new DepartmentDto(department.Id, department.Name));
  }
}

public record DepartmentDto(long Id, string Name);
