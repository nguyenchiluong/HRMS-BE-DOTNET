namespace EmployeeApi.Models;

public class Education
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public string? Institution { get; set; }
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public decimal? Gpa { get; set; }
    public string? Country { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
