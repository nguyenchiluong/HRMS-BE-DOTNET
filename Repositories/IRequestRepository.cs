using EmployeeApi.Models;

namespace EmployeeApi.Repositories;

public interface IRequestRepository
{
    Task<List<Request>> GetRequestsAsync(
        long? employeeId = null,
        string? status = null,
        string? category = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20,
        long? managerId = null,
        bool filterByManagerReports = false,
        long? approverId = null,
        bool filterByApprover = false);

    Task<int> GetRequestsCountAsync(
        long? employeeId = null,
        string? status = null,
        string? category = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        long? managerId = null,
        bool filterByManagerReports = false,
        long? approverId = null,
        bool filterByApprover = false);

    Task<List<long>> GetDirectReportEmployeeIdsAsync(long managerId);

    Task<bool> IsEmployeeUnderManagerAsync(long employeeId, long managerId);

    Task<Request?> GetRequestByIdAsync(int id);

    Task<Request> CreateRequestAsync(Request request);

    Task<Request> UpdateRequestAsync(Request request);

    Task<bool> DeleteRequestAsync(int id);

    Task<Dictionary<string, int>> GetRequestsSummaryByStatusAsync(
        long? employeeId = null,
        string? month = null,
        string? requestType = null);

    Task<Dictionary<string, int>> GetRequestsSummaryByTypeAsync(
        long? employeeId = null,
        string? month = null,
        string? requestType = null);
}
