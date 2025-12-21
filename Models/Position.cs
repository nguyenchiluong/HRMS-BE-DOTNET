using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class Position
{
    [Key]
    [Column("position_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [Column("position_name")]
    public string Title { get; set; } = default!;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("salary")]
    public decimal Salary { get; set; }
}
