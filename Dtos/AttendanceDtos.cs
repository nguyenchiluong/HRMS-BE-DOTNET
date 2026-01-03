namespace EmployeeApi.Dtos;

// Attendance DTOs for MyAttendance page

public class CurrentClockStatusResponseDto
{
    public string Status { get; set; } = default!; // "clocked-in" or "clocked-out"
    public string? ClockInTime { get; set; } // ISO 8601 timestamp (null if not clocked in)
    public int? CurrentWorkingMinutes { get; set; } // Minutes worked since clock in (null if clocked out)
    public string? TodayRecordId { get; set; } // ID of today's attendance record (if exists)
}

public class ClockInResponseDataDto
{
    public string Id { get; set; } = default!; // String format (e.g., "ATT-0001")
    public DateTime Date { get; set; }
    public DateTime ClockInTime { get; set; } // Always present after clock-in
    public DateTime? ClockOutTime { get; set; }
    public int? TotalWorkingMinutes { get; set; }
}

public class ClockInResponseDto
{
    public string Message { get; set; } = default!;
    public ClockInResponseDataDto Data { get; set; } = default!;
}

public class ClockOutResponseDataDto
{
    public string Id { get; set; } = default!; // String format (e.g., "ATT-0001")
    public DateTime Date { get; set; }
    public DateTime ClockInTime { get; set; } // Always present after clock-out
    public DateTime ClockOutTime { get; set; } // Always present after clock-out
    public int TotalWorkingMinutes { get; set; } // Always calculated after clock-out
}

public class ClockOutResponseDto
{
    public string Message { get; set; } = default!;
    public ClockOutResponseDataDto Data { get; set; } = default!;
}

public class AttendanceHistoryRecordDto
{
    public string Id { get; set; } = default!; // String format (e.g., "ATT-0001")
    public DateTime Date { get; set; }
    public DateTime ClockInTime { get; set; } // Always present (required in model)
    public DateTime? ClockOutTime { get; set; } // Nullable if not clocked out yet
    public int? TotalWorkingMinutes { get; set; } // Null if not clocked out yet
}

public class AttendanceHistoryResponseDto
{
    public List<AttendanceHistoryRecordDto> Data { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

