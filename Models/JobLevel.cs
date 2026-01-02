using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class JobLevel
{
    [Key]
    [Column("job_level_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [Column("job_level_name")]
    public string Name { get; set; } = default!;

    [Column("description")]
    public string? Description { get; set; }
}

