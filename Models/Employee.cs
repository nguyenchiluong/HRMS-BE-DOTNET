using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models;

public class Employee
{
    [Key]
    [Column("emp_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    // Basic Info
    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = default!;

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("preferred_name")]
    public string? PreferredName { get; set; }

    [Required]
    [Column("email")]
    public string Email { get; set; } = default!;

    [Column("personal_email")]
    public string? PersonalEmail { get; set; }

    [Column("phone_number")]
    public string? Phone { get; set; }

    [Column("phone_number_2")]
    public string? Phone2 { get; set; }

    // Personal Details
    [Column("sex")]
    public string? Sex { get; set; }

    [Column("date_of_birth", TypeName = "date")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("marital_status")]
    public string? MaritalStatus { get; set; }

    [Column("pronoun")]
    public string? Pronoun { get; set; }

    // Address
    [Column("permanent_address")]
    public string? PermanentAddress { get; set; }

    [Column("current_address")]
    public string? CurrentAddress { get; set; }

    // National ID
    [Column("national_id_country")]
    public string? NationalIdCountry { get; set; }

    [Column("national_id_number")]
    public string? NationalIdNumber { get; set; }

    [Column("national_id_issued_date", TypeName = "date")]
    public DateOnly? NationalIdIssuedDate { get; set; }

    [Column("national_id_expiration_date", TypeName = "date")]
    public DateOnly? NationalIdExpirationDate { get; set; }

    [Column("national_id_issued_by")]
    public string? NationalIdIssuedBy { get; set; }

    // Social Insurance & Tax
    [Column("social_insurance_number")]
    public string? SocialInsuranceNumber { get; set; }

    [Column("tax_id")]
    public string? TaxId { get; set; }

    // Employment Info
    [Column("start_date", TypeName = "date")]
    public DateOnly? StartDate { get; set; }

    [Column("job_level_id")]
    public long? JobLevelId { get; set; }

    [ForeignKey("JobLevelId")]
    public JobLevel? JobLevel { get; set; }

    [Column("employment_type_id")]
    public long? EmploymentTypeId { get; set; }

    [ForeignKey("EmploymentTypeId")]
    public EmploymentType? EmploymentType { get; set; }

    [Column("time_type_id")]
    public long? TimeTypeId { get; set; }

    [ForeignKey("TimeTypeId")]
    public TimeType? TimeType { get; set; }

    [Column("dept_id")]
    public long? DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    [Column("position_id")]
    public long? PositionId { get; set; }

    [ForeignKey("PositionId")]
    public Position? Position { get; set; }

    [Column("manager_id")]
    public long? ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    public Employee? Manager { get; set; }

    // Navigation property for direct reports
    public ICollection<Employee>? DirectReports { get; set; }

    [Column("hr_id")]
    public long? HrId { get; set; }

    [ForeignKey("HrId")]
    public Employee? Hr { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }
}
