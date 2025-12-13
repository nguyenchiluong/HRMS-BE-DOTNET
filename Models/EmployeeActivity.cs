namespace EmployeeApi.Models;

public class EmployeeActivity
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public string ActivityType { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public long PointsEarned { get; set; } = 0;
    public long? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
