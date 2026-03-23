using MediatR;

namespace SmartService.Application.Features.ServiceAgents.Commands.UpdateCapabilities;

public record CapabilityInput(
    Guid CategoryId,
    int MaxComplexityLevel,
    IReadOnlyList<Guid> ServiceIds);

public record UpdateServiceAgentCapabilitiesCommand(
    Guid AgentId,
    IReadOnlyList<CapabilityInput> Capabilities) : IRequest<UpdateServiceAgentCapabilitiesResult>;

public record UpdateServiceAgentCapabilitiesResult(
    Guid AgentId,
    int CapabilityCount);
