using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

/// <summary>
/// Individual timesheet entry representing hours worked on a specific task for a specific week
/// </summary>
public class TimesheetEntry
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the parent request (for approval workflow)
    /// </summary>
    [Required]
    [Column("request_id")]
    public int RequestId { get; set; }

    [ForeignKey(nameof(RequestId))]
    public Request? Request { get; set; }

    /// <summary>
    /// Denormalized employee ID for easier querying
    /// </summary>
    [Required]
    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    /// <summary>
    /// Reference to the timesheet task
    /// </summary>
    [Required]
    [Column("task_id")]
    public int TaskId { get; set; }

    [ForeignKey(nameof(TaskId))]
    public TimesheetTask? Task { get; set; }

    /// <summary>
    /// Entry type: "project" for work entries, "leave" for leave entries
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("entry_type")]
    public string EntryType { get; set; } = "project"; // "project" | "leave"

    /// <summary>
    /// Monday of the week this entry belongs to
    /// </summary>
    [Required]
    [Column("week_start_date", TypeName = "date")]
    public DateOnly WeekStartDate { get; set; }

    /// <summary>
    /// Sunday of the week this entry belongs to
    /// </summary>
    [Required]
    [Column("week_end_date", TypeName = "date")]
    public DateOnly WeekEndDate { get; set; }

    /// <summary>
    /// Total hours worked on this task for this week (max 168 hours = 7 days * 24 hours)
    /// </summary>
    [Required]
    [Column("hours", TypeName = "decimal(5,2)")]
    public decimal Hours { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

