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
    long? PositionId,
    long? DepartmentId,
    long? JobLevelId,
    long? EmploymentTypeId,
    long? TimeTypeId,
    long? ManagerId,
    long? HrId,
    string? DepartmentName,
    string? PositionTitle,
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
    /// <summary>
    /// Personal email address to send the onboarding link to.
    /// Work email will be auto-generated as fullname@hrms.com
    /// </summary>
    public string PersonalEmail { get; set; } = default!;
    public long PositionId { get; set; }
    public long JobLevelId { get; set; }
    public long DepartmentId { get; set; }
    public long EmploymentTypeId { get; set; }
    public long TimeTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public long? ManagerId { get; set; }
    public long? HrId { get; set; }
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
/// Event message for sending email via RabbitMQ.
/// Matches the Java consumer's SendEmailEvent DTO.
/// The producer is responsible for defining all email content.
/// </summary>
public class SendEmailEvent
{
    /// <summary>
    /// The email address to send to
    /// </summary>
    public string EmailToSend { get; set; } = default!;

    /// <summary>
    /// The email subject
    /// </summary>
    public string Subject { get; set; } = default!;

    /// <summary>
    /// The email body content (HTML)
    /// </summary>
    public string HtmlContent { get; set; } = default!;
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

/// <summary>
/// DTO for employee to update their own profile information
/// </summary>
public class UpdateProfileDto
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
}

/// <summary>
/// Simplified DTO for employee table/list view with filtering
/// </summary>
public record FilteredEmployeeDto(
    string Id,
    string FullName,
    string WorkEmail,
    string? Position,
    string? JobLevel,
    string? Department,
    string Status,
    string? EmploymentType,
    string? TimeType,
    long? ManagerId,
    string? ManagerName,
    string? ManagerEmail,
    long? HrId,
    string? HrName,
    string? HrEmail
);

/// <summary>
/// Pagination metadata for employee endpoints
/// </summary>
public record EmployeePaginationDto(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages
);

/// <summary>
/// Paginated response wrapper for employee endpoints
/// </summary>
public record EmployeePaginatedResponse<T>(
    List<T> Data,
    EmployeePaginationDto Pagination
);

/// <summary>
/// Employee statistics DTO
/// </summary>
public record EmployeeStatsDto(
    int Total,
    int Onboarding,
    int Resigned,
    int Managers
);

/// <summary>
/// DTO for manager or HR personnel listing
/// </summary>
public record ManagerOrHrDto(
    long Id,
    string FullName,
    string WorkEmail,
    string? Position,
    long? PositionId,
    string? JobLevel,
    long? JobLevelId,
    string? Department,
    long? DepartmentId,
    string? EmploymentType,
    long? EmploymentTypeId,
    string? TimeType,
    long? TimeTypeId
);

/// <summary>
/// DTO for reassigning supervisors (manager and HR)
/// </summary>
public class ReassignSupervisorsDto
{
    /// <summary>
    /// Manager ID. null means remove manager assignment.
    /// </summary>
    public long? ManagerId { get; set; }

    /// <summary>
    /// HR ID. null means remove HR assignment.
    /// </summary>
    public long? HrId { get; set; }
}