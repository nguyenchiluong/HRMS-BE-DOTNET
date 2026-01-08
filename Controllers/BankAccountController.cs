using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Dtos;
using EmployeeApi.Extensions;
using EmployeeApi.Services;

namespace EmployeeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;

    public BankAccountController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    /// <summary>
    /// Gets the current employee's bank account (single account)
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/bankaccount/me
    ///     Authorization: Bearer {your-jwt-token}
    ///
    /// Returns the first bank account for the authenticated employee as an array.
    /// Returns empty array if no account exists.
    /// </remarks>
    /// <response code="200">Successfully retrieved bank account (as array)</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<BankAccountRecordDto>>> GetMyBankAccounts()
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var bankAccount = await _bankAccountService.GetFirstByEmployeeIdAsync(employeeId.Value);

            // Return as array (even if single item) for compatibility. Frontend will use first item.
            if (bankAccount == null)
            {
                return Ok(new List<BankAccountRecordDto>());
            }

            return Ok(new List<BankAccountRecordDto> { bankAccount });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific bank account for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/bankaccount/me/1234567890/Chase
    ///     Authorization: Bearer {your-jwt-token}
    ///
    /// Returns the bank account with the specified account number and bank name if it belongs to the authenticated employee.
    /// </remarks>
    /// <param name="accountNumber">The bank account number</param>
    /// <param name="bankName">The bank name</param>
    /// <response code="200">Successfully retrieved the bank account</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Bank account not found or does not belong to the user</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("me/{accountNumber}/{bankName}")]
    public async Task<ActionResult<BankAccountRecordDto>> GetMyBankAccount(string accountNumber, string bankName)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var bankAccount = await _bankAccountService.GetByKeysAsync(accountNumber, bankName, employeeId.Value);

            if (bankAccount == null)
            {
                return NotFound(new { message = "Bank account not found" });
            }

            return Ok(bankAccount);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new bank account for the current authenticated user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/bankaccount/me
    ///     Authorization: Bearer {your-jwt-token}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "accountNumber": "1234567890",
    ///       "bankName": "Chase",
    ///       "accountName": "John Doe"
    ///     }
    ///
    /// Creates a new bank account for the authenticated employee.
    /// </remarks>
    /// <param name="dto">The bank account data to create</param>
    /// <response code="201">Successfully created the bank account</response>
    /// <response code="400">Invalid input data or bank account already exists</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Employee not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost("me")]
    public async Task<ActionResult<BankAccountRecordDto>> CreateMyBankAccount([FromBody] CreateBankAccountDto dto)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bankAccount = await _bankAccountService.CreateAsync(employeeId.Value, dto);
            return CreatedAtAction(
                nameof(GetMyBankAccount),
                new { accountNumber = bankAccount.AccountNumber, bankName = bankAccount.BankName },
                bankAccount);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the current employee's bank account
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/bankaccount/me/1
    ///     Authorization: Bearer {your-jwt-token}
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "accountNumber": "1234567890",
    ///       "bankName": "Example Bank",
    ///       "accountName": "John Doe",
    ///       "swiftCode": "CHASUS33",
    ///       "branchCode": "001"
    ///     }
    ///
    /// Updates the bank account with the specified id if it belongs to the authenticated employee.
    /// </remarks>
    /// <param name="id">The bank account id</param>
    /// <param name="dto">The bank account data to update</param>
    /// <response code="200">Successfully updated the bank account</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Bank account not found or does not belong to the user</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("me/{id:long}")]
    public async Task<ActionResult<BankAccountRecordDto>> UpdateMyBankAccount(
        long id,
        [FromBody] UpdateBankAccountDto dto)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bankAccount = await _bankAccountService.UpdateByIdAsync(id, employeeId.Value, dto);

            if (bankAccount == null)
            {
                return NotFound(new { message = "Bank account not found" });
            }

            return Ok(bankAccount);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes the current employee's bank account
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/bankaccount/me/1
    ///     Authorization: Bearer {your-jwt-token}
    ///
    /// Deletes the bank account with the specified id if it belongs to the authenticated employee.
    /// </remarks>
    /// <param name="id">The bank account id to delete</param>
    /// <response code="204">Successfully deleted the bank account</response>
    /// <response code="401">Employee ID not found in JWT token</response>
    /// <response code="404">Bank account not found or does not belong to the user</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpDelete("me/{id:long}")]
    public async Task<IActionResult> DeleteMyBankAccount(long id)
    {
        try
        {
            var employeeId = User.TryGetEmployeeId();

            if (employeeId == null)
            {
                return Unauthorized(new { message = "Employee ID not found in token" });
            }

            var deleted = await _bankAccountService.DeleteByIdAsync(id, employeeId.Value);

            if (!deleted)
            {
                return NotFound(new { message = "Bank account not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ========== Admin Endpoints ==========

    /// <summary>
    /// [Admin Only] Gets all bank accounts for any employee
    /// </summary>
    /// <param name="employeeId">The employee ID to retrieve bank accounts for</param>
    /// <response code="200">Successfully retrieved bank accounts</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Employee not found</response>
    [HttpGet("admin/employee/{employeeId:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<BankAccountRecordDto>>> GetEmployeeBankAccounts(long employeeId)
    {
        try
        {
            var bankAccounts = await _bankAccountService.GetAllByEmployeeIdAsync(employeeId);
            return Ok(bankAccounts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Gets a specific bank account by composite key
    /// </summary>
    /// <param name="accountNumber">The bank account number</param>
    /// <param name="bankName">The bank name</param>
    /// <response code="200">Successfully retrieved the bank account</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Bank account not found</response>
    [HttpGet("admin/{accountNumber}/{bankName}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BankAccountRecordDto>> GetBankAccountByKeys(string accountNumber, string bankName)
    {
        try
        {
            var bankAccount = await _bankAccountService.GetByKeysAsync(accountNumber, bankName, 0); // Admin access

            if (bankAccount == null)
            {
                return NotFound(new { message = "Bank account not found" });
            }

            return Ok(bankAccount);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Creates a bank account for any employee
    /// </summary>
    /// <param name="employeeId">The employee ID to create bank account for</param>
    /// <param name="dto">The bank account data</param>
    /// <response code="201">Successfully created the bank account</response>
    /// <response code="400">Invalid input data or bank account already exists</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Employee not found</response>
    [HttpPost("admin/employee/{employeeId:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BankAccountRecordDto>> CreateEmployeeBankAccount(
        long employeeId,
        [FromBody] CreateBankAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bankAccount = await _bankAccountService.CreateAsync(employeeId, dto);
            return CreatedAtAction(
                nameof(GetBankAccountByKeys),
                new { accountNumber = bankAccount.AccountNumber, bankName = bankAccount.BankName },
                bankAccount);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Updates any bank account
    /// </summary>
    /// <param name="accountNumber">The bank account number</param>
    /// <param name="bankName">The bank name</param>
    /// <param name="dto">The bank account data to update</param>
    /// <response code="200">Successfully updated the bank account</response>
    /// <response code="400">Invalid input data or trying to change to existing account</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Bank account not found</response>
    [HttpPut("admin/{accountNumber}/{bankName}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BankAccountRecordDto>> UpdateBankAccount(
        string accountNumber,
        string bankName,
        [FromBody] UpdateBankAccountDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bankAccount = await _bankAccountService.UpdateAsync(accountNumber, bankName, 0, dto); // Admin access

            if (bankAccount == null)
            {
                return NotFound(new { message = "Bank account not found" });
            }

            return Ok(bankAccount);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin Only] Deletes any bank account
    /// </summary>
    /// <param name="accountNumber">The bank account number</param>
    /// <param name="bankName">The bank name</param>
    /// <response code="204">Successfully deleted the bank account</response>
    /// <response code="403">User does not have admin privileges</response>
    /// <response code="404">Bank account not found</response>
    [HttpDelete("admin/{accountNumber}/{bankName}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBankAccount(string accountNumber, string bankName)
    {
        try
        {
            var deleted = await _bankAccountService.DeleteAsync(accountNumber, bankName, 0); // Admin access

            if (!deleted)
            {
                return NotFound(new { message = "Bank account not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
