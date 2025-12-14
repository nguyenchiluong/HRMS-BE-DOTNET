namespace EmployeeApi.Models;

public class BankAccount
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public string AccountNumber { get; set; } = default!;
    public string BankName { get; set; } = default!;
    public string? AccountName { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
