using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class AttendanceRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public double? TotalHours { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "CHECKED_IN"; // CHECKED_IN, CHECKED_OUT, MISSING_CHECKOUT

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
