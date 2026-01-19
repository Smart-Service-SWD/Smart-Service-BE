using MediatR;

namespace SmartService.Application.Features.ServiceAgents.Commands.Create;

/// <summary>
/// Command to create a new service agent.
/// </summary>
public record CreateServiceAgentCommand(
    string FullName) : IRequest<Guid>;

