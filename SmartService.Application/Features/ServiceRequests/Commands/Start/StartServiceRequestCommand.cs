using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Start;

/// <summary>
/// Command to start a service request that has been assigned to a provider.
/// </summary>
public record StartServiceRequestCommand(Guid ServiceRequestId) : IRequest<Unit>;
