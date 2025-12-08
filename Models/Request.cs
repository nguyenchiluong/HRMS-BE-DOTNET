using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class Request
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string RequestType { get; set; } = default!; // LEAVE, SICK_LEAVE, WFH, TIMESHEET, PROFILE_UPDATE, ID_UPDATE

    [Required]
    public int RequesterEmployeeId { get; set; }

    [ForeignKey(nameof(RequesterEmployeeId))]
    public Employee? Requester { get; set; }

    public int? ApproverEmployeeId { get; set; }

    [ForeignKey(nameof(ApproverEmployeeId))]
    public Employee? Approver { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, CANCELLED

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [Required]
    public string Reason { get; set; } = default!;

    [Column(TypeName = "json")]
    public string? Payload { get; set; } // JSON string for type-specific data

    [MaxLength(500)]
    public string? ApprovalComment { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
