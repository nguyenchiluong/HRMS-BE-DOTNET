using EmployeeApi.Dtos;
using EmployeeApi.Models;

namespace EmployeeApi.Services;

public interface IRequestService
{
    Task<PaginatedResponseDto<RequestDto>> GetRequestsAsync(
        int? employeeId = null,
        string? status = null,
        string? requestType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20);

    Task<RequestDetailsDto?> GetRequestByIdAsync(int id);

    Task<RequestDto> CreateRequestAsync(CreateRequestDto dto, int requesterEmployeeId);

    Task<RequestDto> UpdateRequestAsync(int id, UpdateRequestDto dto, int requesterEmployeeId);

    Task<bool> CancelRequestAsync(int id, int requesterEmployeeId);

    Task<RequestDto> ApproveRequestAsync(int id, int approverEmployeeId, string? comment);

    Task<RequestDto> RejectRequestAsync(int id, int approverEmployeeId, string reason);

    Task<RequestsSummaryDto> GetRequestsSummaryAsync(
        int? employeeId = null,
        string? month = null,
        string? requestType = null);
}
