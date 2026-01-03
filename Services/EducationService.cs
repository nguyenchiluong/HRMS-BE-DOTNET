using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services;

public class EducationService : IEducationService
{
    private readonly IEducationRepository _educationRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public EducationService(
        IEducationRepository educationRepository,
        IEmployeeRepository employeeRepository)
    {
        _educationRepository = educationRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<IReadOnlyList<EducationRecordDto>> GetAllByEmployeeIdAsync(long employeeId)
    {
        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        var educations = await _educationRepository.GetByEmployeeIdAsync(employeeId);
        return educations.Select(MapToDto).ToList();
    }

    public async Task<EducationRecordDto?> GetByIdAsync(long id, long employeeId)
    {
        var education = await _educationRepository.GetByIdAndEmployeeIdAsync(id, employeeId);
        return education == null ? null : MapToDto(education);
    }

    public async Task<EducationRecordDto> CreateAsync(long employeeId, CreateEducationDto dto)
    {
        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
        }

        var education = new Education
        {
            EmployeeId = employeeId,
            Degree = dto.Degree,
            FieldOfStudy = dto.FieldOfStudy,
            Gpa = dto.Gpa,
            Country = dto.Country
        };

        await _educationRepository.AddAsync(education);
        await _educationRepository.SaveChangesAsync();

        return MapToDto(education);
    }

    public async Task<EducationRecordDto?> UpdateAsync(long id, long employeeId, UpdateEducationDto dto)
    {
        var education = await _educationRepository.GetByIdAndEmployeeIdAsync(id, employeeId);
        if (education == null)
        {
            return null;
        }

        // Update only provided fields (null means keep existing value)
        if (dto.Degree != null)
            education.Degree = dto.Degree;
        
        // For optional fields, check if they were explicitly set
        if (dto.FieldOfStudy != null)
            education.FieldOfStudy = dto.FieldOfStudy;
        
        if (dto.Gpa.HasValue)
            education.Gpa = dto.Gpa;
        
        if (dto.Country != null)
            education.Country = dto.Country;

        _educationRepository.Update(education);
        await _educationRepository.SaveChangesAsync();

        return MapToDto(education);
    }

    public async Task<bool> DeleteAsync(long id, long employeeId)
    {
        var education = await _educationRepository.GetByIdAndEmployeeIdAsync(id, employeeId);
        if (education == null)
        {
            return false;
        }

        _educationRepository.Remove(education);
        await _educationRepository.SaveChangesAsync();

        return true;
    }

    private static EducationRecordDto MapToDto(Education education)
    {
        return new EducationRecordDto
        {
            Id = education.Id,
            EmployeeId = education.EmployeeId,
            Degree = education.Degree,
            FieldOfStudy = education.FieldOfStudy,
            Gpa = education.Gpa,
            Country = education.Country
        };
    }
}
