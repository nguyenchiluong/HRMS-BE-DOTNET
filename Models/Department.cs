namespace EmployeeApi.Models;

public class Department
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public long? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
