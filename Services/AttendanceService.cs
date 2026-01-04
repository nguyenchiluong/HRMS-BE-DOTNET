using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;

    public AttendanceService(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }
    public async Task<CurrentClockStatusResponseDto> GetCurrentClockStatusAsync(long employeeId)
    {
        var todayRecord = await _attendanceRepository.GetTodayAttendanceAsync(employeeId);

        if (todayRecord == null)
        {
            return new CurrentClockStatusResponseDto
            {
                Status = "clocked-out",
                ClockInTime = null,
                CurrentWorkingMinutes = null,
                TodayRecordId = null
            };
        }

        // If clocked in but not clocked out
        if (!todayRecord.CheckOutTime.HasValue)
        {
            var now = DateTime.UtcNow;
            var workingMinutes = (int)Math.Round((now - todayRecord.CheckInTime).TotalMinutes);

            return new CurrentClockStatusResponseDto
            {
                Status = "clocked-in",
                ClockInTime = todayRecord.CheckInTime.ToString("O"), // ISO 8601 format
                CurrentWorkingMinutes = workingMinutes,
                TodayRecordId = FormatAttendanceId(todayRecord.Id)
            };
        }

        // If already clocked out today
        return new CurrentClockStatusResponseDto
        {
            Status = "clocked-out",
            ClockInTime = todayRecord.CheckInTime.ToString("O"),
            CurrentWorkingMinutes = null,
            TodayRecordId = FormatAttendanceId(todayRecord.Id)
        };
    }

    public async Task<ClockInResponseDto> ClockInAsync(long employeeId)
    {
        // Check if already clocked in today
        var existingRecord = await _attendanceRepository.GetTodayAttendanceAsync(employeeId);
        if (existingRecord != null && !existingRecord.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Already clocked in today");
        }

        var now = DateTime.UtcNow;
        AttendanceRecord record;

        // Create new record (CheckInTime is required, so we always create a new record)
        record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = now.Date,
            CheckInTime = now,
            Status = "CHECKED_IN",
            CreatedAt = now
        };
        record = await _attendanceRepository.CheckInAsync(record);

        return new ClockInResponseDto
        {
            Message = "Successfully clocked in",
            Data = new ClockInResponseDataDto
            {
                Id = FormatAttendanceId(record.Id),
                Date = record.Date,
                ClockInTime = record.CheckInTime,
                ClockOutTime = record.CheckOutTime,
                TotalWorkingMinutes = null
            }
        };
    }

    public async Task<ClockOutResponseDto> ClockOutAsync(long employeeId)
    {
        // Get today's attendance record
        var record = await _attendanceRepository.GetTodayAttendanceAsync(employeeId);
        if (record == null)
        {
            throw new InvalidOperationException("Must clock in before clocking out");
        }

        if (record.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Already clocked out today");
        }

        var now = DateTime.UtcNow;
        record.CheckOutTime = now;
        record.TotalHours = (now - record.CheckInTime).TotalHours;
        record.Status = "CHECKED_OUT";

        var updatedRecord = await _attendanceRepository.CheckOutAsync(record);

        var totalWorkingMinutes = (int)Math.Round((updatedRecord.CheckOutTime!.Value - updatedRecord.CheckInTime).TotalMinutes);

        return new ClockOutResponseDto
        {
            Message = "Successfully clocked out",
            Data = new ClockOutResponseDataDto
            {
                Id = FormatAttendanceId(updatedRecord.Id),
                Date = updatedRecord.Date,
                ClockInTime = updatedRecord.CheckInTime,
                ClockOutTime = updatedRecord.CheckOutTime!.Value,
                TotalWorkingMinutes = totalWorkingMinutes
            }
        };
    }

    public async Task<AttendanceHistoryResponseDto> GetAttendanceHistoryForEmployeeAsync(
        long employeeId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int limit = 7)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (limit < 1) limit = 7;
        if (limit > 100) limit = 100;

        var records = await _attendanceRepository.GetAttendanceHistoryAsync(
            employeeId, startDate, endDate, page, limit);

        var totalCount = await _attendanceRepository.GetAttendanceCountAsync(
            employeeId, startDate, endDate);

        var recordDtos = records.Select(MapToAttendanceHistoryRecordDto).ToList();

        return new AttendanceHistoryResponseDto
        {
            Data = recordDtos,
            Pagination = new PaginationDto
            {
                Page = page,
                Limit = limit,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            }
        };
    }

    private AttendanceHistoryRecordDto MapToAttendanceHistoryRecordDto(AttendanceRecord record)
    {
        int? totalWorkingMinutes = null;
        if (record.CheckOutTime.HasValue)
        {
            totalWorkingMinutes = (int)Math.Round((record.CheckOutTime.Value - record.CheckInTime).TotalMinutes);
        }

        return new AttendanceHistoryRecordDto
        {
            Id = FormatAttendanceId(record.Id),
            Date = record.Date,
            ClockInTime = record.CheckInTime,
            ClockOutTime = record.CheckOutTime,
            TotalWorkingMinutes = totalWorkingMinutes
        };
    }

    private static string FormatAttendanceId(long id)
    {
        return $"ATT-{id:D4}";
    }
}
