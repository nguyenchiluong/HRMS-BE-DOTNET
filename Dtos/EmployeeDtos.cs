namespace EmployeeApi.Dtos;

public record EmployeeDto(
    long Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateTime? HireDate,
    DateTime? BirthDate,
    string? PositionTitle,
    string? DepartmentName,
    bool IsActive,
    string? JobStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public long? PositionId { get; set; }
    public long? DepartmentId { get; set; }
    public long? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? JobStatus { get; set; }
}