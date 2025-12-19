using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetTodayAttendanceAsync(long employeeId);

    Task<AttendanceRecord> CheckInAsync(AttendanceRecord record);

    Task<AttendanceRecord> CheckOutAsync(AttendanceRecord record);

    Task<List<AttendanceRecord>> GetAttendanceHistoryAsync(
        long? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20);

    Task<int> GetAttendanceCountAsync(
        long? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);

    Task<AttendanceRecord?> GetAttendanceByIdAsync(long id);
}
