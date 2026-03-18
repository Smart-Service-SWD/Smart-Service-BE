using MediatR;

namespace SmartService.Application.Features.ServiceAgents.Commands.SetActiveStatus;

public record SetServiceAgentActiveStatusCommand(
    Guid AgentId,
    Guid ActorUserId,
    bool CanManageAnyAgent,
    bool IsActive) : IRequest<ServiceAgentActiveStatusResult>;

public record ServiceAgentActiveStatusResult(
    Guid AgentId,
    bool IsActive);
