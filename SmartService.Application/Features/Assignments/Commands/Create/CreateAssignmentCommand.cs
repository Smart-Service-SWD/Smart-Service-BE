using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.Assignments.Commands.Create;

/// <summary>
/// Command to create a new assignment.
/// </summary>
public record CreateAssignmentCommand(
    Guid ServiceRequestId,
    Guid AgentId,
    Money EstimatedCost) : IRequest<Guid>;

