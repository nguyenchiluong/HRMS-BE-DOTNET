using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Models;

public class BankAccount
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("account_number")]
    public string AccountNumber { get; set; } = default!;

    [Column("bank_name")]
    public string BankName { get; set; } = default!;

    [Column("account_name")]
    public string AccountName { get; set; } = default!;

    [Column("swift_code")]
    [StringLength(11)]
    public string? SwiftCode { get; set; }

    [Column("branch_code")]
    [StringLength(20)]
    public string? BranchCode { get; set; }

    [Column("emp_id")]
    public long EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = default!;
}
