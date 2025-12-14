namespace EmployeeApi.Models;

public class CampaignParticipant
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public Campaign Campaign { get; set; } = default!;
    public long EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "JOINED"; // JOINED, COMPLETED, CANCELLED
    public long PointsEarned { get; set; } = 0;
}
