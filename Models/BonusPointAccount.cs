namespace EmployeeApi.Models;

public class BonusPointAccount
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public long Balance { get; set; } = 0;
    public string Currency { get; set; } = "POINT";
    public string Status { get; set; } = "ACTIVE"; // PENDING, ACTIVE, etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
