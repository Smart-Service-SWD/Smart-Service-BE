using SmartService.Domain.ValueObjects;

namespace SmartService.Domain.Entities;

/// <summary>
/// Represents a specific capability of a service agent within the system.
/// 
/// An AgentCapability defines:
/// - Which ServiceCategory the agent is qualified to handle
/// - The maximum ServiceComplexity level the agent is allowed to work with
/// 
/// This entity is used by the matching and assignment logic to ensure
/// that only qualified agents are suggested or assigned to a service request.
/// 
/// AgentCapability does NOT contain business logic for matching itself,
/// but acts as a rule boundary for eligibility checks.
/// </summary>
public class AgentCapability
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public ServiceComplexity MaxComplexity { get; private set; } = null;

    private AgentCapability() { }

    private AgentCapability(Guid categoryId, ServiceComplexity maxComplexity)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        MaxComplexity = maxComplexity;
    }

    public static AgentCapability Create(Guid categoryId, ServiceComplexity maxComplexity)
        => new(categoryId, maxComplexity);
}
