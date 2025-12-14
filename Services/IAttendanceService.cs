using EmployeeApi.Dtos;

namespace EmployeeApi.Services;

public interface IAttendanceService
{
    Task<CheckInResponseDto> CheckInAsync(long employeeId, LocationDto? location);

    Task<CheckOutResponseDto> CheckOutAsync(long employeeId);

    Task<PaginatedResponseDto<AttendanceRecordDto>> GetAttendanceHistoryAsync(
        long? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20);
}
