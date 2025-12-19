using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class Employee
{
    [Key]
    [Column("emp_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = default!;

    [Required]
    [Column("email")]
    public string Email { get; set; } = default!;

    [Column("phone_number")]
    public string? Phone { get; set; }

    [Column("permanent_address")]
    public string? PermanentAddress { get; set; }

    [Column("current_address")]
    public string? CurrentAddress { get; set; }

    [Column("start_date", TypeName = "date")]
    public DateOnly? StartDate { get; set; }

    [Column("job_level")]
    public string? JobLevel { get; set; }

    [Column("employee_type")]
    public string? EmployeeType { get; set; }

    [Column("time_type")]
    public string? TimeType { get; set; }

    [Column("dept_id")]
    public long? DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    [Column("position_id")]
    public long? PositionId { get; set; }

    [ForeignKey("PositionId")]
    public Position? Position { get; set; }

    [Column("manager_id")]
    public long? ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    public Employee? Manager { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }
}
