using MediatR;

namespace SmartService.Application.Features.ServiceAgents.Commands.Deactivate;

/// <summary>
/// Command to deactivate a service agent.
/// </summary>
public record DeactivateServiceAgentCommand(
    Guid AgentId) : IRequest<Unit>;

