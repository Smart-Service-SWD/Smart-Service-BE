using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Update;

/// <summary>
/// Command to update a service request's description.
/// Can only update requests that haven't been assigned yet.
/// </summary>
public record UpdateServiceRequestCommand(
    Guid ServiceRequestId,
    string Description) : IRequest<Unit>;
