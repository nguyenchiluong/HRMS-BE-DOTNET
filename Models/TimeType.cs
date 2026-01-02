using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class TimeType
{
    [Key]
    [Column("time_type_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [Column("time_type_name")]
    public string Name { get; set; } = default!;

    [Column("description")]
    public string? Description { get; set; }
}

