using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface ITeamMembersService
{
    /// <summary>
    /// Get summary metrics for team members (lightweight, can be cached)
    /// </summary>
    Task<TeamMembersSummaryDto> GetTeamMembersSummaryAsync(long managerId);

    /// <summary>
    /// Get paginated list of team members with filters
    /// </summary>
    Task<TeamMembersResponseDto> GetTeamMembersAsync(
        long managerId,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? department = null,
        string? status = null,
        string? position = null);
}
