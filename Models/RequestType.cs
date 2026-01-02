using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

/// <summary>
/// Request type lookup table - allows dynamic management of request types
/// </summary>
public class RequestTypeLookup
{
    [Key]
    [Column("request_type_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("request_type_code")]
    public string Code { get; set; } = default!; // e.g., "PAID_LEAVE", "TIMESHEET_WEEKLY"

    [Required]
    [MaxLength(50)]
    [Column("request_type_name")]
    public string Name { get; set; } = default!; // e.g., "Paid Leave"

    [Required]
    [MaxLength(20)]
    [Column("category")]
    public string Category { get; set; } = default!; // "time-off", "timesheet", "profile", "other"

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("requires_approval")]
    public bool RequiresApproval { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

