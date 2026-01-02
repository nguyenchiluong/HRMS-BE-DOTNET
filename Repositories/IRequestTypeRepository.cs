using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IRequestTypeRepository
{
    Task<List<RequestTypeLookup>> GetAllRequestTypesAsync(bool activeOnly = true);
    Task<RequestTypeLookup?> GetRequestTypeByCodeAsync(string code);
    Task<RequestTypeLookup?> GetRequestTypeByIdAsync(long id);
}

