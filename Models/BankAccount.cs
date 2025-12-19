using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Models;

[PrimaryKey(nameof(AccountNumber), nameof(BankName))]
public class BankAccount
{
    [Column("account_number")]
    public string AccountNumber { get; set; } = default!;

    [Column("bank_name")]
    public string BankName { get; set; } = default!;

    [Column("account_name")]
    public string? AccountName { get; set; }

    [Column("emp_id")]
    public long EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = default!;
}
