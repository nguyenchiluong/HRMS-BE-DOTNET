using EmployeeApi.Dtos;
using EmployeeApi.Models;

namespace EmployeeApi.Services;

public interface IRequestService
{
    Task<PaginatedResponseDto<RequestDto>> GetRequestsAsync(
        long? employeeId = null,
        string? status = null,
        string? requestType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20);

    Task<RequestDetailsDto?> GetRequestByIdAsync(int id);

    Task<RequestDto> CreateRequestAsync(CreateRequestDto dto, long requesterEmployeeId);

    Task<RequestDto> UpdateRequestAsync(int id, UpdateRequestDto dto, long requesterEmployeeId);

    Task<bool> CancelRequestAsync(int id, long requesterEmployeeId);

    Task<RequestDto> ApproveRequestAsync(int id, long approverEmployeeId, string? comment);

    Task<RequestDto> RejectRequestAsync(int id, long approverEmployeeId, string reason);

    Task<RequestsSummaryDto> GetRequestsSummaryAsync(
        long? employeeId = null,
        string? month = null,
        string? requestType = null);
}
