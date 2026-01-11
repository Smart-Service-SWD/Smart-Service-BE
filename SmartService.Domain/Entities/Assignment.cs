using SmartService.Domain.ValueObjects;

namespace SmartService.Domain.Entities;

/// <summary>
/// Represents the assignment of a service request
/// to a specific service agent.
/// 
/// This entity records who was assigned,
/// by whom, and when the assignment occurred.
/// 
/// Assignment is immutable once created
/// to preserve audit integrity.
/// </summary>
public class Assignment
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public Guid AgentId { get; private set; }
    public Money EstimatedCost { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private Assignment() { }

    private Assignment(Guid serviceRequestId, Guid agentId, Money estimatedCost)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        AgentId = agentId;
        EstimatedCost = estimatedCost;
        AssignedAt = DateTime.UtcNow;
    }

    public static Assignment Create(
        Guid serviceRequestId,
        Guid agentId,
        Money estimatedCost)
    {
        return new Assignment(serviceRequestId, agentId, estimatedCost);
    }
}
