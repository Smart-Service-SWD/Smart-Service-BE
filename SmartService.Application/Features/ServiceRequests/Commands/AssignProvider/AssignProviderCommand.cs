using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;

/// <summary>
/// Command to assign a service provider to a service request.
/// </summary>
public record AssignProviderCommand(
    Guid ServiceRequestId,
    Guid ProviderId,
    Money EstimatedCost) : IRequest<Unit>;

