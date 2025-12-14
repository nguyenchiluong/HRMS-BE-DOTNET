using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Models;

public class Employee
{
    public long Id { get; set; }
    public string? EmployeeNumber { get; set; }
    [Required]
    public string FirstName { get; set; } = default!;
    [Required]
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }

    public long? PositionId { get; set; }
    public Position? Position { get; set; }

    public long? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public long? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public bool IsActive { get; set; } = true;
    public string? JobStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
