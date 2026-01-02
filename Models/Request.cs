using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeApi.Models.Enums;

namespace EmployeeApi.Models;

public class Request
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    // Foreign key to request_type table
    [Required]
    [Column("request_type_id")]
    public long RequestTypeId { get; set; }

    [ForeignKey(nameof(RequestTypeId))]
    public RequestTypeLookup? RequestTypeLookup { get; set; }

    [Required]
    [Column("requester_employee_id")]
    public long RequesterEmployeeId { get; set; }

    [ForeignKey(nameof(RequesterEmployeeId))]
    public Employee? Requester { get; set; }

    [Column("approver_employee_id")]
    public long? ApproverEmployeeId { get; set; }

    [ForeignKey(nameof(ApproverEmployeeId))]
    public Employee? Approver { get; set; }

    [Required]
    [Column("status")]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Required]
    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Column("effective_from")]
    public DateTime? EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateTime? EffectiveTo { get; set; }

    [Required]
    [Column("reason")]
    public string Reason { get; set; } = default!;

    [Column("payload", TypeName = "jsonb")]
    public string? Payload { get; set; } // JSON string for type-specific data

    [MaxLength(500)]
    [Column("approval_comment")]
    public string? ApprovalComment { get; set; }

    [MaxLength(500)]
    [Column("rejection_reason")]
    public string? RejectionReason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for timesheet entries
    public ICollection<TimesheetEntry>? TimesheetEntries { get; set; }
}
