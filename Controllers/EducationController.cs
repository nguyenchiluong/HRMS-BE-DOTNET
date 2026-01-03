using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Dtos;
using EmployeeApi.Extensions;
using EmployeeApi.Services;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;

    public EducationController(IEducationService educationService)
    {
        _educationService = educationService;
    }

    /// <summary>
    /// Gets all education records for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/education/me
    ///     Authorization: Bearer {your-jwt-token}
    ///
    /// Returns a list of all education records belonging to the authenticated employee.
    /// </remarks>
    /// <response code="200">Successfully retrieved education records</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<EducationRecordDto>>> GetMyEducations()
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();
            
            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var educations = await _educationService.GetAllByEmployeeIdAsync(employeeId.Value);
            return Ok(educations);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific education record for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/education/me/123
    ///     Authorization: Bearer {your-jwt-token}
    ///
    /// Returns the education record with the specified ID if it belongs to the authenticated employee.
    /// </remarks>
    /// <param name="id">The education record ID</param>
    /// <response code="200">Successfully retrieved the education record</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Education record not found or does not belong to the user</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("me/{id:long}")]
    public async Task<ActionResult<EducationRecordDto>> GetMyEducation(long id)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();
            
            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var education = await _educationService.GetByIdAsync(id, employeeId.Value);
            
            if (education == null)
            {
                return NotFound(new { message = "Education record not found" });
            }

            return Ok(education);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new education record for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/education/me
    ///     Authorization: Bearer {your-jwt-token}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "degree": "Bachelor of Science in Computer Science",
    ///       "fieldOfStudy": "Computer Science",
    ///       "gpa": 3.8,
    ///       "country": "United States"
    ///     }
    ///
    /// Creates a new education record for the authenticated employee.
    /// </remarks>
    /// <param name="dto">The education data to create</param>
    /// <response code="201">Successfully created the education record</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost("me")]
    public async Task<ActionResult<EducationRecordDto>> CreateMyEducation([FromBody] CreateEducationDto dto)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();
            
            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var education = await _educationService.CreateAsync(employeeId.Value, dto);
            return CreatedAtAction(
                nameof(GetMyEducation), 
                new { id = education.Id }, 
                education);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing education record for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/education/me/123
    ///     Authorization: Bearer {your-jwt-token}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "degree": "Master of Science in Computer Science",
    ///       "fieldOfStudy": "Artificial Intelligence",
    ///       "gpa": 3.9
    ///     }
    ///
    /// Updates the education record with the specified ID if it belongs to the authenticated employee.
    /// Only the fields provided will be updated.
    /// </remarks>
    /// <param name="id">The education record ID to update</param>
    /// <param name="dto">The education data to update</param>
    /// <response code="200">Successfully updated the education record</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Education record not found or does not belong to the user</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("me/{id:long}")]
    public async Task<ActionResult<EducationRecordDto>> UpdateMyEducation(long id, [FromBody] UpdateEducationDto dto)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();
            
            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var education = await _educationService.UpdateAsync(id, employeeId.Value, dto);
            
            if (education == null)
            {
                return NotFound(new { message = "Education record not found" });
            }

            return Ok(education);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an education record for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/education/me/123
    ///     Authorization: Bearer {your-jwt-token}
    ///
    /// Deletes the education record with the specified ID if it belongs to the authenticated employee.
    /// </remarks>
    /// <param name="id">The education record ID to delete</param>
    /// <response code="204">Successfully deleted the education record</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Education record not found or does not belong to the user</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpDelete("me/{id:long}")]
    public async Task<IActionResult> DeleteMyEducation(long id)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();
            
            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var deleted = await _educationService.DeleteAsync(id, employeeId.Value);
            
            if (!deleted)
            {
                return NotFound(new { message = "Education record not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ========== Admin Endpoints ==========

    /// <summary>
    /// [Admin Only] Gets all education records for any employee
    /// </summary>
    /// <param name="employeeId">The employee ID to retrieve education records for</param>
    /// <response code="200">Successfully retrieved education records</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Employee not found</response>
    [HttpGet("admin/employee/{employeeId:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<EducationRecordDto>>> GetEmployeeEducations(long employeeId)
    {
        try
        {
            var educations = await _educationService.GetAllByEmployeeIdAsync(employeeId);
            return Ok(educations);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Gets a specific education record by ID
    /// </summary>
    /// <param name="id">The education record ID</param>
    /// <response code="200">Successfully retrieved the education record</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Education record not found</response>
    [HttpGet("admin/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<EducationRecordDto>> GetEducationById(long id)
    {
        try
        {
            var education = await _educationService.GetByIdAsync(id, 0); // Admin can access any record
            
            if (education == null)
            {
                return NotFound(new { message = "Education record not found" });
            }

            return Ok(education);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Creates an education record for any employee
    /// </summary>
    /// <param name="employeeId">The employee ID to create education record for</param>
    /// <param name="dto">The education data</param>
    /// <response code="201">Successfully created the education record</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Employee not found</response>
    [HttpPost("admin/employee/{employeeId:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<EducationRecordDto>> CreateEmployeeEducation(
        long employeeId, 
        [FromBody] CreateEducationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var education = await _educationService.CreateAsync(employeeId, dto);
            return CreatedAtAction(
                nameof(GetEducationById), 
                new { id = education.Id }, 
                education);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Updates any education record
    /// </summary>
    /// <param name="id">The education record ID</param>
    /// <param name="dto">The education data to update</param>
    /// <response code="200">Successfully updated the education record</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Education record not found</response>
    [HttpPut("admin/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<EducationRecordDto>> UpdateEducation(
        long id, 
        [FromBody] UpdateEducationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var education = await _educationService.UpdateAsync(id, 0, dto); // Admin can update any record
            
            if (education == null)
            {
                return NotFound(new { message = "Education record not found" });
            }

            return Ok(education);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Deletes any education record
    /// </summary>
    /// <param name="id">The education record ID</param>
    /// <response code="204">Successfully deleted the education record</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Education record not found</response>
    [HttpDelete("admin/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteEducation(long id)
    {
        try
        {
            var deleted = await _educationService.DeleteAsync(id, 0); // Admin can delete any record
            
            if (!deleted)
            {
                return NotFound(new { message = "Education record not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
