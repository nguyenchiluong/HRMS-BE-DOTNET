using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class Education
{
  [Key]
  [Column("id")]
  public long Id { get; set; }

  [Column("emp_id")]
  public long EmployeeId { get; set; }

  [ForeignKey("EmployeeId")]
  public Employee Employee { get; set; } = default!;

  [Required]
  [Column("degree")]
  public string Degree { get; set; } = default!;

  [Column("field_of_study")]
  public string? FieldOfStudy { get; set; }

  [Column("gpa")]
  public double? Gpa { get; set; }

  [Column("country")]
  public string? Country { get; set; }
}
