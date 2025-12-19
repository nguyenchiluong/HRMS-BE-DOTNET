using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class Department
{
    [Key]
    [Column("dept_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [Column("dept_name")]
    public string Name { get; set; } = default!;

    [Column("location")]
    public string? Location { get; set; }

    [Column("manager_id")]
    public long? ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    public Employee? Manager { get; set; }
}
