using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Dtos;

/// <summary>
/// DTO for bank account record with full details
/// </summary>
public record BankAccountRecordDto
{
    public string AccountNumber { get; init; } = default!;
    public string BankName { get; init; } = default!;
    public string? AccountName { get; init; }
    public long EmployeeId { get; init; }
}

public record CreateBankAccountDto
{
    [Required(ErrorMessage = "Account number is required")]
    [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
    public string AccountNumber { get; init; } = default!;

    [Required(ErrorMessage = "Bank name is required")]
    [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
    public string BankName { get; init; } = default!;

    [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
    public string? AccountName { get; init; }
}

public record UpdateBankAccountDto
{
    [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
    public string? AccountNumber { get; init; }

    [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
    public string? BankName { get; init; }

    [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
    public string? AccountName { get; init; }
}
