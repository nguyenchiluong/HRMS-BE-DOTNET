using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Dtos;

/// <summary>
/// DTO for education record with full details including ID
/// </summary>
public record EducationRecordDto
{
    public long Id { get; init; }
    public long EmployeeId { get; init; }
    public string Degree { get; init; } = default!;
    public string? FieldOfStudy { get; init; }
    public double? Gpa { get; init; }
    public string? Country { get; init; }
}

public record CreateEducationDto
{
    [Required(ErrorMessage = "Degree is required")]
    [StringLength(200, ErrorMessage = "Degree cannot exceed 200 characters")]
    public string Degree { get; init; } = default!;

    [StringLength(200, ErrorMessage = "Field of study cannot exceed 200 characters")]
    public string? FieldOfStudy { get; init; }

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0")]
    public double? Gpa { get; init; }

    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string? Country { get; init; }
}

public record UpdateEducationDto
{
    [StringLength(200, ErrorMessage = "Degree cannot exceed 200 characters")]
    public string? Degree { get; init; }

    [StringLength(200, ErrorMessage = "Field of study cannot exceed 200 characters")]
    public string? FieldOfStudy { get; init; }

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0")]
    public double? Gpa { get; init; }

    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string? Country { get; init; }
}
