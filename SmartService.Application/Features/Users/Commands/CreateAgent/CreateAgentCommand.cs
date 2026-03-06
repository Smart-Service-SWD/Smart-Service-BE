using MediatR;

namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Command to create a new agent user with their service capabilities.
/// When creating an agent, at least one capability must be provided.
/// Each capability must reference an existing ServiceCategory and one or more ServiceDefinitions.
/// </summary>
public record CreateAgentCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    IReadOnlyList<CapabilityInput> Capabilities) : IRequest<Guid>;

