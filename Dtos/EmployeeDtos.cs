namespace EmployeeApi.Dtos;

public record EmployeeDto(
    long Id,
    string FullName,
    string? FirstName,
    string? LastName,
    string? PreferredName,
    string Email,
    string? PersonalEmail,
    string? Phone,
    string? Phone2,
    string? Sex,
    DateOnly? DateOfBirth,
    string? MaritalStatus,
    string? Pronoun,
    string? PermanentAddress,
    string? CurrentAddress,
    string? NationalIdCountry,
    string? NationalIdNumber,
    DateOnly? NationalIdIssuedDate,
    DateOnly? NationalIdExpirationDate,
    string? NationalIdIssuedBy,
    string? SocialInsuranceNumber,
    string? TaxId,
    DateOnly? StartDate,
    string? PositionTitle,
    string? DepartmentName,
    string? JobLevel,
    string? EmployeeType,
    string? TimeType,
    string? Status,
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
    public string? Country { get; set; }
    public string? Institution { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public double? Gpa { get; set; }
}

/// <summary>
/// DTO for bank account during onboarding
/// </summary>
public class BankAccountDto
{
    public string BankName { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string? AccountName { get; set; }
}

/// <summary>
/// DTO for National ID information
/// </summary>
public class NationalIdDto
{
    public string? Country { get; set; }
    public string? Number { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? IssuedBy { get; set; }
}

/// <summary>
/// Event message for sending onboarding email to new employee.
/// Matches the Java consumer's SendEmailEvent DTO.
/// </summary>
public class SendEmailEvent
{
    public long EmployeeId { get; set; }
    public string PersonalEmail { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? WorkEmail { get; set; }
}

/// <summary>
/// DTO for new hire to complete their onboarding with personal details
/// </summary>
public class OnboardDto
{
    // Personal details
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PreferredName { get; set; }
    public string? Sex { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Pronoun { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }

    // Address
    public string? PermanentAddress { get; set; }
    public string? CurrentAddress { get; set; }

    // National ID
    public NationalIdDto? NationalId { get; set; }

    // Social Insurance & Tax
    public string? SocialInsuranceNumber { get; set; }
    public string? TaxId { get; set; }

    // Education history (optional)
    public List<EducationDto>? Education { get; set; }

    // Financial details
    public BankAccountDto? BankAccount { get; set; }

    // Comment
    public string? Comment { get; set; }
}