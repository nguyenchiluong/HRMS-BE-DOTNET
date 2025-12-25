namespace EmployeeApi.Models.Enums;

/// <summary>
/// Represents the status of an employee in the HR system
/// </summary>
public enum EmployeeStatus
{
    /// <summary>
    /// Employee profile created, waiting to complete onboarding form
    /// </summary>
    PendingOnboarding,

    /// <summary>
    /// Onboarding completed, employee is active
    /// </summary>
    Active,

    /// <summary>
    /// Employee is on leave (maternity, sabbatical, etc.)
    /// </summary>
    OnLeave,

    /// <summary>
    /// Employee is temporarily inactive
    /// </summary>
    Inactive,

    /// <summary>
    /// Employee has been terminated/offboarded
    /// </summary>
    Terminated
}

