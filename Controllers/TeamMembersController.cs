using EmployeeApi.Dtos;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/v1/team-members")]
[Authorize]
public class TeamMembersController : ControllerBase
{
    private readonly ITeamMembersService _teamMembersService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<TeamMembersController> _logger;

    public TeamMembersController(
        ITeamMembersService teamMembersService,
        IUserContextService userContextService,
        ILogger<TeamMembersController> logger)
    {
        _teamMembersService = teamMembersService;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// Get summary metrics for team members (lightweight, can be cached)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<TeamMembersSummaryDto>> GetTeamMembersSummary()
    {
        try
        {
            // Get current user's employee ID from JWT token
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var userRole = _userContextService.GetRoleFromClaims(User);

            // Verify user has MANAGER role
            var isManagerOrAdmin = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
                                || userRole.Equals("MANAGER", StringComparison.OrdinalIgnoreCase);

            if (!isManagerOrAdmin)
            {
                return StatusCode(403, new { error = "Access denied. Manager role required." });
            }

            var summary = await _teamMembersService.GetTeamMembersSummaryAsync(currentEmployeeId);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new { error = "Unauthorized", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members summary");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get paginated list of team members with filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TeamMembersResponseDto>> GetTeamMembers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        [FromQuery] string? position = null)
    {
        try
        {
            // Validate pagination
            if (page < 1)
            {
                return BadRequest(new { error = "Invalid page number" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Page size must be between 1 and 100" });
            }

            // Get current user's employee ID from JWT token
            var currentEmployeeId = await _userContextService.GetEmployeeIdFromClaimsAsync(User);
            var userRole = _userContextService.GetRoleFromClaims(User);

            // Verify user has MANAGER role
            var isManagerOrAdmin = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)
                                || userRole.Equals("MANAGER", StringComparison.OrdinalIgnoreCase);

            if (!isManagerOrAdmin)
            {
                return StatusCode(403, new { error = "Access denied. Manager role required." });
            }

            var result = await _teamMembersService.GetTeamMembersAsync(
                currentEmployeeId,
                page,
                pageSize,
                search,
                department,
                status,
                position);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(401, new { error = "Unauthorized", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }
}
