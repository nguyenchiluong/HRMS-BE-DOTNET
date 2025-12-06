namespace EmployeeApi.Dtos;
public record EmployeeDto(
    int Id, 
    string FullName, 
    string Email,   
    string Position, 
    DateTime StartDate,
    string Status, 
    string JobLevel, 
    string Department,
    string EmploymentType, 
    string TimeType, 
    DateTime LastUpdated
);

public class CreateEmployeeDto
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Position { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string JobLevel { get; set; } = default!;
    public string Department { get; set; } = default!;
    public string EmploymentType { get; set; } = default!;
    public string TimeType { get; set; } = default!;
}