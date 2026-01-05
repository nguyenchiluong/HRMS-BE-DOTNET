using EmployeeApi.Dtos;
using EmployeeApi.Models;

namespace EmployeeApi.Helpers;

/// <summary>
/// Mapper for converting between Employee entities and DTOs
/// </summary>
public static class EmployeeMapper
{
    /// <summary>
    /// Converts an Employee entity to EmployeeDto
    /// </summary>
    public static EmployeeDto ToDto(Employee e) =>
        new EmployeeDto(
            e.Id,
            e.FullName,
            e.FirstName,
            e.LastName,
            e.PreferredName,
            e.Email,
            e.PersonalEmail,
            e.Phone,
            e.Phone2,
            e.Sex,
            e.DateOfBirth,
            e.MaritalStatus,
            e.Pronoun,
            e.PermanentAddress,
            e.CurrentAddress,
            e.NationalIdCountry,
            e.NationalIdNumber,
            e.NationalIdIssuedDate,
            e.NationalIdExpirationDate,
            e.NationalIdIssuedBy,
            e.SocialInsuranceNumber,
            e.TaxId,
            e.StartDate,
            e.PositionId,
            e.DepartmentId,
            e.JobLevelId,
            e.EmploymentTypeId,
            e.TimeTypeId,
            e.ManagerId,
            e.Department?.Name,
            e.Position?.Title,
            e.Status,
            e.CreatedAt,
            e.UpdatedAt
        );

    /// <summary>
    /// Updates employee personal details from OnboardDto
    /// </summary>
    public static void UpdateFromOnboardDto(Employee employee, OnboardDto input)
    {
        // Update personal details
        employee.FirstName = input.FirstName;
        employee.LastName = input.LastName;
        employee.PreferredName = input.PreferredName;
        employee.Sex = input.Sex;
        employee.DateOfBirth = input.DateOfBirth;
        employee.MaritalStatus = input.MaritalStatus;
        employee.Pronoun = input.Pronoun;
        employee.PersonalEmail = input.PersonalEmail;
        employee.Phone = input.Phone;
        employee.Phone2 = input.Phone2;

        // Update address
        employee.PermanentAddress = input.PermanentAddress;
        employee.CurrentAddress = input.CurrentAddress;

        // Update National ID
        if (input.NationalId != null)
        {
            employee.NationalIdCountry = input.NationalId.Country;
            employee.NationalIdNumber = input.NationalId.Number;
            employee.NationalIdIssuedDate = input.NationalId.IssuedDate;
            employee.NationalIdExpirationDate = input.NationalId.ExpirationDate;
            employee.NationalIdIssuedBy = input.NationalId.IssuedBy;
        }

        // Update Social Insurance & Tax
        employee.SocialInsuranceNumber = input.SocialInsuranceNumber;
        employee.TaxId = input.TaxId;

        employee.UpdatedAt = DateTime.Now;
    }
}

