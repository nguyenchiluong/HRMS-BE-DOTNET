using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Dtos;

/// <summary>
/// DTO for bank account record with full details
/// </summary>
public record BankAccountRecordDto
{
    public long Id { get; init; }
    public string AccountNumber { get; init; } = default!;
    public string BankName { get; init; } = default!;
    public string AccountName { get; init; } = default!;
    public string? SwiftCode { get; init; }
    public string? BranchCode { get; init; }
    public long EmployeeId { get; init; }
}

public record CreateBankAccountDto
{
    [Required(ErrorMessage = "Account number is required")]
    [RegularExpression(@"^\d{8,20}$", ErrorMessage = "Account number must be 8-20 digits only")]
    public string AccountNumber { get; init; } = default!;

    [Required(ErrorMessage = "Bank name is required")]
    [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
    public string BankName { get; init; } = default!;

    [Required(ErrorMessage = "Account name is required")]
    [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
    public string AccountName { get; init; } = default!;

    [RegularExpression(@"^(?i)[A-Z0-9]{8,11}$", ErrorMessage = "SWIFT code must be 8-11 alphanumeric characters")]
    [StringLength(11, ErrorMessage = "SWIFT code cannot exceed 11 characters")]
    public string? SwiftCode { get; init; }

    [StringLength(20, ErrorMessage = "Branch code cannot exceed 20 characters")]
    public string? BranchCode { get; init; }
}

public record UpdateBankAccountDto
{
    [RegularExpression(@"^\d{8,20}$", ErrorMessage = "Account number must be 8-20 digits only")]
    public string? AccountNumber { get; init; }

    [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
    public string? BankName { get; init; }

    [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
    public string? AccountName { get; init; }

    [RegularExpression(@"^(?i)[A-Z0-9]{8,11}$", ErrorMessage = "SWIFT code must be 8-11 alphanumeric characters")]
    [StringLength(11, ErrorMessage = "SWIFT code cannot exceed 11 characters")]
    public string? SwiftCode { get; init; }

    [StringLength(20, ErrorMessage = "Branch code cannot exceed 20 characters")]
    public string? BranchCode { get; init; }
}
