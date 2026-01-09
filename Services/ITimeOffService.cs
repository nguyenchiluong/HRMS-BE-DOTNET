using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface ITimeOffService
{
    Task<TimeOffRequestResponseDto> SubmitTimeOffRequestAsync(SubmitTimeOffRequestDto dto, long employeeId, List<string>? attachmentUrls = null, string? userRole = null);
    Task<LeaveBalancesResponseDto> GetLeaveBalancesAsync(long employeeId, int year);
    Task<TimeOffRequestHistoryResponseDto> GetTimeOffRequestHistoryAsync(
        long employeeId,
        int page = 1,
        int limit = 10,
        string? status = null,
        string? type = null);
    Task<TimeOffRequestResponseDto> CancelTimeOffRequestAsync(string requestId, long employeeId, string? comment);
    Task<int> CalculateDurationAsync(DateOnly startDate, DateOnly endDate);
    Task<string> GenerateRequestIdAsync();
}

