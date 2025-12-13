using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeApi.Models.Enums;

namespace EmployeeApi.Models;

public class Request
{
    [Key]
    public int Id { get; set; }

    [Required]
    public RequestType RequestType { get; set; }

    [Required]
    public long RequesterEmployeeId { get; set; }

    [ForeignKey(nameof(RequesterEmployeeId))]
    public Employee? Requester { get; set; }

    public long? ApproverEmployeeId { get; set; }

    [ForeignKey(nameof(ApproverEmployeeId))]
    public Employee? Approver { get; set; }

    [Required]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [Required]
    public string Reason { get; set; } = default!;

    [Column(TypeName = "jsonb")]
    public string? Payload { get; set; } // JSON string for type-specific data

    [MaxLength(500)]
    public string? ApprovalComment { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
