using EmployeeApi.Dtos;
using EmployeeApi.Models;
using EmployeeApi.Models.Enums;
using EmployeeApi.Repositories;
using EmployeeApi.Helpers;
using System.Text.Json;

namespace EmployeeApi.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IRequestTypeRepository _requestTypeRepository;

    public RequestService(
        IRequestRepository requestRepository,
        IRequestTypeRepository requestTypeRepository)
    {
        _requestRepository = requestRepository;
        _requestTypeRepository = requestTypeRepository;
    }

    public async Task<PaginatedResponseDto<RequestDto>> GetRequestsAsync(
        long? employeeId = null,
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

    public async Task<RequestDto> CreateRequestAsync(CreateRequestDto dto, long requesterEmployeeId)
    {
        // Look up request type from database
        var normalizedType = dto.RequestType.ToUpper().Replace("-", "_");
        var requestTypeLookup = await _requestTypeRepository.GetRequestTypeByCodeAsync(normalizedType);
        if (requestTypeLookup == null)
        {
            throw new ArgumentException($"Invalid request type: {dto.RequestType}");
        }

        // Ensure DateTime values are UTC for PostgreSQL compatibility
        var effectiveFrom = dto.EffectiveFrom.HasValue
            ? DateTime.SpecifyKind(dto.EffectiveFrom.Value, DateTimeKind.Utc)
            : (DateTime?)null;
        var effectiveTo = dto.EffectiveTo.HasValue
            ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var request = new Request
        {
            RequestTypeId = requestTypeLookup.Id,
            RequesterEmployeeId = requesterEmployeeId,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Reason = dto.Reason,
            Payload = dto.Payload.HasValue ? JsonSerializer.Serialize(dto.Payload.Value) : null,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRequest = await _requestRepository.CreateRequestAsync(request);
        return MapToRequestDto(createdRequest);
    }

    public async Task<RequestDto> UpdateRequestAsync(int id, UpdateRequestDto dto, long requesterEmployeeId)
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

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be updated");
        }

        if (dto.EffectiveFrom.HasValue)
        {
            request.EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom.Value, DateTimeKind.Utc);
        }

        if (dto.EffectiveTo.HasValue)
        {
            request.EffectiveTo = DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc);
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

    public async Task<bool> CancelRequestAsync(int id, long requesterEmployeeId)
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

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be cancelled");
        }

        request.Status = RequestStatus.Cancelled;
        await _requestRepository.UpdateRequestAsync(request);
        return true;
    }

    public async Task<RequestDto> ApproveRequestAsync(int id, long approverEmployeeId, string? comment)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be approved");
        }

        request.Status = RequestStatus.Approved;
        request.ApproverEmployeeId = approverEmployeeId;
        request.ApprovalComment = comment;

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);
        return MapToRequestDto(updatedRequest);
    }

    public async Task<RequestDto> RejectRequestAsync(int id, long approverEmployeeId, string reason)
    {
        var request = await _requestRepository.GetRequestByIdAsync(id);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != RequestStatus.Pending)
        {
            throw new InvalidOperationException("Only PENDING requests can be rejected");
        }

        request.Status = RequestStatus.Rejected;
        request.ApproverEmployeeId = approverEmployeeId;
        request.RejectionReason = reason;

        var updatedRequest = await _requestRepository.UpdateRequestAsync(request);
        return MapToRequestDto(updatedRequest);
    }

    public async Task<RequestsSummaryDto> GetRequestsSummaryAsync(
        long? employeeId = null,
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
            RequestType = request.RequestTypeLookup?.Code ?? "UNKNOWN",
            RequesterEmployeeId = request.RequesterEmployeeId,
            ApproverEmployeeId = request.ApproverEmployeeId,
            Status = request.Status.ToApiString(),
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
            RequestType = request.RequestTypeLookup?.Code ?? "UNKNOWN",
            RequesterEmployeeId = request.RequesterEmployeeId,
            ApproverEmployeeId = request.ApproverEmployeeId,
            Status = request.Status.ToApiString(),
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
                Department = request.Requester.Department?.Name
            } : null,
            Approver = request.Approver != null ? new EmployeeSummaryDto
            {
                Id = request.Approver.Id,
                Name = request.Approver.FullName,
                Email = request.Approver.Email,
                Department = request.Approver.Department?.Name
            } : null,
            Payload = !string.IsNullOrEmpty(request.Payload)
                ? JsonSerializer.Deserialize<JsonElement>(request.Payload)
                : null,
            ApprovalComment = request.ApprovalComment,
            RejectionReason = request.RejectionReason
        };
    }
}
