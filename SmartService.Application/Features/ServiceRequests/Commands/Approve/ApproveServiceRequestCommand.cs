using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Approve;

/// <summary>
/// Command to approve a service request that is pending review.
/// </summary>
public record ApproveServiceRequestCommand(Guid ServiceRequestId) : IRequest<Unit>;
