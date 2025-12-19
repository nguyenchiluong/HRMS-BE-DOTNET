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

    public async Task<CheckInResponseDto> CheckInAsync(long employeeId, LocationDto? location)
    {
        // Check if already checked in today
        var existingRecord = await _attendanceRepository.GetTodayAttendanceAsync(employeeId);
        if (existingRecord != null)
        {
            throw new InvalidOperationException("Already checked in today");
        }

        var now = DateTime.UtcNow;
        var record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = now.Date,
            CheckInTime = now,
            Latitude = location?.Latitude,
            Longitude = location?.Longitude,
            Status = "CHECKED_IN",
            CreatedAt = now
        };

        var createdRecord = await _attendanceRepository.CheckInAsync(record);

        return new CheckInResponseDto
        {
            Message = "Checked in successfully",
            CheckInTime = createdRecord.CheckInTime
        };
    }

    public async Task<CheckOutResponseDto> CheckOutAsync(long employeeId)
    {
        // Get today's attendance record
        var record = await _attendanceRepository.GetTodayAttendanceAsync(employeeId);
        if (record == null)
        {
            throw new InvalidOperationException("No check-in record found for today");
        }

        if (record.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Already checked out today");
        }

        var now = DateTime.UtcNow;
        record.CheckOutTime = now;
        record.TotalHours = (now - record.CheckInTime).TotalHours;
        record.Status = "CHECKED_OUT";

        var updatedRecord = await _attendanceRepository.CheckOutAsync(record);

        return new CheckOutResponseDto
        {
            Message = "Checked out successfully",
            CheckOutTime = updatedRecord.CheckOutTime!.Value,
            TotalHours = updatedRecord.TotalHours!.Value
        };
    }

    public async Task<PaginatedResponseDto<AttendanceRecordDto>> GetAttendanceHistoryAsync(
        long? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20)
    {
        var records = await _attendanceRepository.GetAttendanceHistoryAsync(
            employeeId, dateFrom, dateTo, page, limit);

        var totalCount = await _attendanceRepository.GetAttendanceCountAsync(
            employeeId, dateFrom, dateTo);

        var recordDtos = records.Select(MapToAttendanceRecordDto).ToList();

        return new PaginatedResponseDto<AttendanceRecordDto>
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

    private AttendanceRecordDto MapToAttendanceRecordDto(AttendanceRecord record)
    {
        return new AttendanceRecordDto
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            Date = record.Date,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            TotalHours = record.TotalHours,
            Location = (record.Latitude.HasValue && record.Longitude.HasValue)
                ? new LocationDto
                {
                    Latitude = record.Latitude.Value,
                    Longitude = record.Longitude.Value
                }
                : null,
            Status = record.Status,
            CreatedAt = record.CreatedAt
        };
    }
}
