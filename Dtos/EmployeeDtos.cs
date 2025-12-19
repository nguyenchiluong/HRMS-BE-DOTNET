namespace EmployeeApi.Dtos;

public record EmployeeDto(
    long Id,
    string FullName,
    string Email,
    string? Phone,
    DateOnly? StartDate,
    string? PositionTitle,
    string? DepartmentName,
    string? JobLevel,
    string? EmployeeType,
    string? TimeType,
    string? Status,
    string? PermanentAddress,
    string? CurrentAddress,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);

public class CreateEmployeeDto
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public DateOnly? StartDate { get; set; }
    public long? PositionId { get; set; }
    public long? DepartmentId { get; set; }
    public long? ManagerId { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO for admin to create initial employee profile for a new hire
/// </summary>
public class InitialProfileDto
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public long PositionId { get; set; }
    public string JobLevel { get; set; } = default!;
    public long DepartmentId { get; set; }
    public string EmployeeType { get; set; } = default!;
    public string TimeType { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public long? ManagerId { get; set; }
}

/// <summary>
/// DTO for employee education during onboarding
/// </summary>
public class EducationDto
{
    public string Degree { get; set; } = default!;
    public string? FieldOfStudy { get; set; }
    public double? Gpa { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// DTO for new hire to complete their onboarding with personal details
/// </summary>
public class OnboardDto
{
    // Personal details
    public string? Phone { get; set; }

    // Address
    public string? PermanentAddress { get; set; }
    public string? CurrentAddress { get; set; }

    // Education history
    public List<EducationDto>? Education { get; set; }
}