using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;

/// <summary>
/// Command to evaluate the complexity of an existing service request.
/// </summary>
public record EvaluateServiceComplexityCommand(
    Guid ServiceRequestId,
    ServiceComplexity Complexity) : IRequest<Unit>;

