using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Cancel;

/// <summary>
/// Command to cancel a service request with a reason.
/// </summary>
public record CancelServiceRequestCommand(
    Guid ServiceRequestId,
    string CancellationReason) : IRequest<Unit>;
