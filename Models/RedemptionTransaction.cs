namespace EmployeeApi.Models;

public class RedemptionTransaction
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public BonusPointAccount Account { get; set; } = default!;
    public long Amount { get; set; }
    public string? RewardReference { get; set; }
    public long? RedeemedByEmployeeId { get; set; }
    public Employee? RedeemedByEmployee { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, COMPLETED, FAILED
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
