using MediatR;
using SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

namespace SmartService.Application.Features.ServiceRequests.Commands.CompleteServiceRequest;

/// <summary>
/// Command to complete an in-progress service request.
/// </summary>
public record CompleteServiceRequestCommand(Guid ServiceRequestId) : IRequest<ServiceRequestStatusResult>;
