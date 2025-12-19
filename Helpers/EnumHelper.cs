using EmployeeApi.Models.Enums;

namespace EmployeeApi.Helpers;

public static class EnumHelper
{
    public static RequestType ParseRequestType(string value)
    {
        return value.ToUpper() switch
        {
            "LEAVE" => RequestType.Leave,
            "SICK_LEAVE" or "SICKLEAVE" => RequestType.SickLeave,
            "WFH" => RequestType.WFH,
            "TIMESHEET" => RequestType.Timesheet,
            "PROFILE_UPDATE" or "PROFILEUPDATE" => RequestType.ProfileUpdate,
            "ID_UPDATE" or "IDUPDATE" => RequestType.IdUpdate,
            _ => Enum.Parse<RequestType>(value, ignoreCase: true)
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
            RequestType.Leave => "LEAVE",
            RequestType.SickLeave => "SICK_LEAVE",
            RequestType.WFH => "WFH",
            RequestType.Timesheet => "TIMESHEET",
            RequestType.ProfileUpdate => "PROFILE_UPDATE",
            RequestType.IdUpdate => "ID_UPDATE",
            _ => requestType.ToString().ToUpper()
        };
    }

    public static string ToApiString(this RequestStatus status)
    {
        return status.ToString().ToUpper();
    }
}
