using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

/// <summary>
/// Master table for timesheet tasks (e.g., Daily Learning, Project A, Project B)
/// Each task has a unique task code for reference
/// </summary>
public class TimesheetTask
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Unique task code (e.g., "DL001", "PROJ-A", "PROJ-B")
    /// Used for identification in reports and exports
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("task_code")]
    public string TaskCode { get; set; } = default!;

    /// <summary>
    /// Display name of the task (e.g., "Daily Learning", "Project Alpha")
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("task_name")]
    public string TaskName { get; set; } = default!;

    /// <summary>
    /// Optional description of the task
    /// </summary>
    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Task type: "project" for work tasks, "leave" for leave-related entries
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("task_type")]
    public string TaskType { get; set; } = "project"; // "project" | "leave"

    /// <summary>
    /// Whether this task is currently active and available for selection
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<TimesheetEntry>? TimesheetEntries { get; set; }
}

