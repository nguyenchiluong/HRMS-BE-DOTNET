namespace EmployeeApi.Models;

public class Employee
{
    public int Id { get; set; } // Auto-generated Employee ID
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Position { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/Active
    public string JobLevel { get; set; } = default!;
    public string Department { get; set; } = default!;
    public string EmploymentType { get; set; } = default!; // e.g., Permanent
    public string TimeType { get; set; } = default!; // e.g., Remote
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
