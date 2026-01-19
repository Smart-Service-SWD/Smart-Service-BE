using MediatR;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.AgentCapabilities.Commands.Create;

/// <summary>
/// Command to add a capability to a service agent.
/// </summary>
public record CreateAgentCapabilityCommand(
    Guid CategoryId,
    ServiceComplexity MaxComplexity) : IRequest<Guid>;

