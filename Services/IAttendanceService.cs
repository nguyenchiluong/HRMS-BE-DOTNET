using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IAttendanceService
{
    Task<CurrentClockStatusResponseDto> GetCurrentClockStatusAsync(long employeeId);
    Task<ClockInResponseDto> ClockInAsync(long employeeId);
    Task<ClockOutResponseDto> ClockOutAsync(long employeeId);
    Task<AttendanceHistoryResponseDto> GetAttendanceHistoryForEmployeeAsync(
        long employeeId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int limit = 7);
}
