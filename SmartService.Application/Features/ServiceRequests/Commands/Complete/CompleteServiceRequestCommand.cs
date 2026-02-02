using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Complete;

/// <summary>
/// Command to complete a service request that is in progress.
/// </summary>
public record CompleteServiceRequestCommand(Guid ServiceRequestId) : IRequest<Unit>;
