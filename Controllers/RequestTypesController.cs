using EmployeeApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/request-types")]
[Authorize]
public class RequestTypesController : ControllerBase
{
    private readonly IRequestTypeRepository _requestTypeRepository;

    public RequestTypesController(IRequestTypeRepository requestTypeRepository)
    {
        _requestTypeRepository = requestTypeRepository;
    }

    /// <summary>
    /// Get all available request types
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RequestTypesResponseDto>> GetRequestTypes([FromQuery] bool activeOnly = true)
    {
        var requestTypes = await _requestTypeRepository.GetAllRequestTypesAsync(activeOnly);

        var requestTypeDtos = requestTypes.Select(rt => new RequestTypeDto
        {
            Id = rt.Id,
            Value = rt.Code, // Uppercase snake_case (e.g., "PAID_LEAVE")
            Category = rt.Category,
            Description = rt.Description ?? rt.Name,
            Name = rt.Name,
            IsActive = rt.IsActive,
            RequiresApproval = rt.RequiresApproval
        }).ToList();

        return Ok(new RequestTypesResponseDto
        {
            RequestTypes = requestTypeDtos
        });
    }
}

public class RequestTypeDto
{
    public long Id { get; set; }
    public string Value { get; set; } = default!; // Uppercase snake_case (e.g., "PAID_LEAVE")
    public string Category { get; set; } = default!; // "time-off", "timesheet", "profile", "other"
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool RequiresApproval { get; set; }
}

public class RequestTypesResponseDto
{
    public List<RequestTypeDto> RequestTypes { get; set; } = new();
}

