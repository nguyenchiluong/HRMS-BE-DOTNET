using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Repositories;
using System.Text.Json;

namespace EmployeeApi.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;

    public RequestService(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<PaginatedResponseDto<RequestDto>> GetRequestsAsync(
        int? employeeId = null,
        string? status = null,
        string? requestType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20)
    {
        var requests = await _requestRepository.GetRequestsAsync(
            employeeId, status, requestType, dateFrom, dateTo, page, limit);

        var totalCount = await _requestRepository.GetRequestsCountAsync(
            employeeId, status, requestType, dateFrom, dateTo);

        var requestDtos = requests.Select(MapToRequestDto).ToList();

        return new PaginatedResponseDto<RequestDto>
        {
            Data = requestDtos,
            Pagination = new PaginationDto
            {
                Page = page,
                Limit = limit,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / limit)
            }
        };
    }

    public async Task<RequestDetailsDto?> GetRequestByIdAsync(int id)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            return null;
        }

        return MapToRequestDetailsDto(request);
    }

    public async Task<RequestDto> CreateRequestAsync(CreateRequestDto dto, int requesterEmployeeId)
    {
        var request = new Request
        {
            RequestType = dto.RequestType,
            RequesterEmployeeId = requesterEmployeeId,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            Reason = dto.Reason,
            Payload = dto.Payload.HasValue ? JsonSerializer.Serialize(dto.Payload.Value) : null,
            Status = "PENDING",
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRequest = await _requestRepository.CreateRequestAsync(request);
        return MapToRequestDto(createdRequest);
    }

    public async Task<RequestDto> UpdateRequestAsync(int id, UpdateRequestDto dto, int requesterEmployeeId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.RequesterEmployeeId != requesterEmployeeId)
        {
            throw new UnauthorizedAccessException("You can only update your own requests");
        }

        if (request.Status != "PENDING")
        {
            throw new InvalidOperationException("Only PENDING requests can be updated");
        }

        if (dto.EffectiveFrom.HasValue)
        {
            request.EffectiveFrom = dto.EffectiveFrom;
        }

        if (dto.EffectiveTo.HasValue)
        {
            request.EffectiveTo = dto.EffectiveTo;
        }

        if (!string.IsNullOrEmpty(dto.Reason))
        {
            request.Reason = dto.Reason;
        }

        if (dto.Payload.HasValue)
        {
            request.Payload = JsonSerializer.Serialize(dto.Payload.Value);
        }

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);
        return MapToRequestDto(updatedRequest);
    }

    public async Task<bool> CancelRequestAsync(int id, int requesterEmployeeId)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.RequesterEmployeeId != requesterEmployeeId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own requests");
        }

        if (request.Status != "PENDING")
        {
            throw new InvalidOperationException("Only PENDING requests can be cancelled");
        }

        request.Status = "CANCELLED";
        await _requestRepository.UpdateRequestAsync(request);
        return true;
    }

    public async Task<RequestDto> ApproveRequestAsync(int id, int approverEmployeeId, string? comment)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != "PENDING")
        {
            throw new InvalidOperationException("Only PENDING requests can be approved");
        }

        request.Status = "APPROVED";
        request.ApproverEmployeeId = approverEmployeeId;
        request.ApprovalComment = comment;

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);
        return MapToRequestDto(updatedRequest);
    }

    public async Task<RequestDto> RejectRequestAsync(int id, int approverEmployeeId, string reason)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != "PENDING")
        {
            throw new InvalidOperationException("Only PENDING requests can be rejected");
        }

        request.Status = "REJECTED";
        request.ApproverEmployeeId = approverEmployeeId;
        request.RejectionReason = reason;

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);
        return MapToRequestDto(updatedRequest);
    }

    public async Task<RequestsSummaryDto> GetRequestsSummaryAsync(
        int? employeeId = null,
        string? month = null,
        string? requestType = null)
    {
        var byStatus = await _requestRepository.GetRequestsSummaryByStatusAsync(employeeId, month, requestType);
        var byType = await _requestRepository.GetRequestsSummaryByTypeAsync(employeeId, month, requestType);

        var total = byStatus.Values.Sum();

        return new RequestsSummaryDto
        {
            Total = total,
            ByStatus = new StatusCountDto
            {
                Pending = byStatus.GetValueOrDefault("pending", 0),
                Approved = byStatus.GetValueOrDefault("approved", 0),
                Rejected = byStatus.GetValueOrDefault("rejected", 0),
                Cancelled = byStatus.GetValueOrDefault("cancelled", 0)
            },
            ByType = byType
        };
    }

    private RequestDto MapToRequestDto(Request request)
    {
        return new RequestDto
        {
            Id = request.Id,
            RequestType = request.RequestType,
            RequesterEmployeeId = request.RequesterEmployeeId,
            ApproverEmployeeId = request.ApproverEmployeeId,
            Status = request.Status,
            RequestedAt = request.RequestedAt,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Reason = request.Reason,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
    }

    private RequestDetailsDto MapToRequestDetailsDto(Request request)
    {
        return new RequestDetailsDto
        {
            Id = request.Id,
            RequestType = request.RequestType,
            RequesterEmployeeId = request.RequesterEmployeeId,
            ApproverEmployeeId = request.ApproverEmployeeId,
            Status = request.Status,
            RequestedAt = request.RequestedAt,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Reason = request.Reason,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            Requester = request.Requester != null ? new EmployeeSummaryDto
            {
                Id = request.Requester.Id,
                Name = request.Requester.FullName,
                Email = request.Requester.Email,
                Department = request.Requester.Department
            } : null,
            Approver = request.Approver != null ? new EmployeeSummaryDto
            {
                Id = request.Approver.Id,
                Name = request.Approver.FullName,
                Email = request.Approver.Email,
                Department = request.Approver.Department
            } : null,
            Payload = !string.IsNullOrEmpty(request.Payload) 
                ? JsonSerializer.Deserialize<JsonElement>(request.Payload) 
                : null,
            ApprovalComment = request.ApprovalComment,
            RejectionReason = request.RejectionReason
        };
    }
}
