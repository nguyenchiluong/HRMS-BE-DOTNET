using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class LeaveBalance
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("balance_type")]
    public string BalanceType { get; set; } = default!; // "Annual Leave", "Sick Leave", "Parental Leave", "Other Leave"

    [Required]
    [Column("year")]
    public int Year { get; set; }

    [Required]
    [Column("total", TypeName = "decimal(5,2)")]
    public decimal Total { get; set; } = 0;

    [Required]
    [Column("used", TypeName = "decimal(5,2)")]
    public decimal Used { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

