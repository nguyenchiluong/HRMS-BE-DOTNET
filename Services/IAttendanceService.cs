using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IAttendanceService
{
    Task<CheckInResponseDto> CheckInAsync(int employeeId, LocationDto? location);

    Task<CheckOutResponseDto> CheckOutAsync(int employeeId);

    Task<PaginatedResponseDto<AttendanceRecordDto>> GetAttendanceHistoryAsync(
        int? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20);
}
