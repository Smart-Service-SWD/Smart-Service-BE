using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

/// <summary>
/// Command to start work on an assigned service request.
/// </summary>
public record StartServiceRequestCommand(Guid ServiceRequestId) : IRequest<ServiceRequestStatusResult>;

public record ServiceRequestStatusResult(Guid ServiceRequestId, string Status);
