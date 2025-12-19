namespace EmployeeApi.Models;

public class TransferTransaction
{
    public long Id { get; set; }
    public long FromAccountId { get; set; }
    public BonusPointAccount FromAccount { get; set; } = default!;
    public long ToAccountId { get; set; }
    public BonusPointAccount ToAccount { get; set; } = default!;
    public long Amount { get; set; }
    public long? InitiatedByEmployeeId { get; set; }
    public Employee? InitiatedByEmployee { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, COMPLETED, FAILED
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
