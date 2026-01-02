using EmployeeApi.Models.Enums;

namespace EmployeeApi.Helpers;

public static class EnumHelper
{
    public static RequestType ParseRequestType(string value)
    {
        return value.ToUpper().Replace("-", "_") switch
        {
            // Time-Off types
            "PAID_LEAVE" or "PAIDLEAVE" => RequestType.PaidLeave,
            "UNPAID_LEAVE" or "UNPAIDLEAVE" => RequestType.UnpaidLeave,
            "PAID_SICK_LEAVE" or "PAIDSICKLEAVE" => RequestType.PaidSickLeave,
            "UNPAID_SICK_LEAVE" or "UNPAIDSICKLEAVE" => RequestType.UnpaidSickLeave,
            "WFH" or "WORK_FROM_HOME" or "WORKFROMHOME" => RequestType.WorkFromHome,
            
            // Timesheet types
            "TIMESHEET_WEEKLY" or "TIMESHEETWEEKLY" => RequestType.TimesheetWeekly,
            
            // Profile update types
            "PROFILE_UPDATE" or "PROFILEUPDATE" => RequestType.ProfileUpdate,
            "ID_UPDATE" or "IDUPDATE" => RequestType.IdUpdate,
            
            _ => Enum.Parse<RequestType>(value.Replace("-", "_"), ignoreCase: true)
        };
    }

    public static RequestStatus ParseRequestStatus(string value)
    {
        return value.ToUpper() switch
        {
            "PENDING" => RequestStatus.Pending,
            "APPROVED" => RequestStatus.Approved,
            "REJECTED" => RequestStatus.Rejected,
            "CANCELLED" => RequestStatus.Cancelled,
            _ => Enum.Parse<RequestStatus>(value, ignoreCase: true)
        };
    }

    public static string ToApiString(this RequestType requestType)
    {
        return requestType switch
        {
            // Time-Off types
            RequestType.PaidLeave => "PAID_LEAVE",
            RequestType.UnpaidLeave => "UNPAID_LEAVE",
            RequestType.PaidSickLeave => "PAID_SICK_LEAVE",
            RequestType.UnpaidSickLeave => "UNPAID_SICK_LEAVE",
            RequestType.WorkFromHome => "WFH",
            
            // Timesheet types
            RequestType.TimesheetWeekly => "TIMESHEET_WEEKLY",
            
            // Profile update types
            RequestType.ProfileUpdate => "PROFILE_UPDATE",
            RequestType.IdUpdate => "ID_UPDATE",
            
            _ => requestType.ToString().ToUpper()
        };
    }


    public static string ToApiString(this RequestStatus status)
    {
        return status.ToString().ToUpper();
    }

    public static EmployeeStatus ParseEmployeeStatus(string value)
    {
        return value.ToUpper() switch
        {
            "PENDING_ONBOARDING" or "PENDINGONBOARDING" => EmployeeStatus.PendingOnboarding,
            "ACTIVE" => EmployeeStatus.Active,
            "ON_LEAVE" or "ONLEAVE" => EmployeeStatus.OnLeave,
            "INACTIVE" => EmployeeStatus.Inactive,
            "TERMINATED" => EmployeeStatus.Terminated,
            _ => Enum.Parse<EmployeeStatus>(value, ignoreCase: true)
        };
    }

    public static string ToApiString(this EmployeeStatus status)
    {
        return status switch
        {
            EmployeeStatus.PendingOnboarding => "PENDING_ONBOARDING",
            EmployeeStatus.Active => "ACTIVE",
            EmployeeStatus.OnLeave => "ON_LEAVE",
            EmployeeStatus.Inactive => "INACTIVE",
            EmployeeStatus.Terminated => "TERMINATED",
            _ => status.ToString().ToUpper()
        };
    }
}
