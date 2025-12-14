using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Models;

public class Position
{
    public long Id { get; set; }
    public string? Code { get; set; }
    [Required]
    public string Title { get; set; } = default!;
    public string? Grade { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
