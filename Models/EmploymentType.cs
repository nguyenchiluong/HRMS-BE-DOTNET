using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class EmploymentType
{
    [Key]
    [Column("employment_type_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    [Column("employment_type_name")]
    public string Name { get; set; } = default!;

    [Column("description")]
    public string? Description { get; set; }
}


