namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Input DTO for specifying an agent capability during agent creation.
/// Each capability links the agent to a ServiceCategory and specific ServiceDefinitions.
/// ServiceIds = list of ServiceDefinition.Id values that the agent is authorized to handle.
/// </summary>
public record CapabilityInput(
    Guid CategoryId,
    int MaxComplexityLevel,
    IReadOnlyList<Guid> ServiceIds);
