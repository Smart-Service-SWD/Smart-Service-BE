using MediatR;
using SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

namespace SmartService.Application.Features.ServiceRequests.Commands.CancelServiceRequest;

/// <summary>
/// Command to cancel a customer-owned service request before staff confirms complexity.
/// </summary>
public record CancelServiceRequestCommand(
    Guid ServiceRequestId,
    Guid CustomerId) : IRequest<ServiceRequestStatusResult>;
