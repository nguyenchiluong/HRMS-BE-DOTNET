namespace EmployeeApi.Models.Enums;

public enum RequestType
{
    // Time-Off types
    PaidLeave,
    UnpaidLeave,
    PaidSickLeave,
    UnpaidSickLeave,
    WorkFromHome,
    
    // Timesheet types
    TimesheetWeekly,
    
    // Profile update types (legacy)
    ProfileUpdate,
    IdUpdate
}
