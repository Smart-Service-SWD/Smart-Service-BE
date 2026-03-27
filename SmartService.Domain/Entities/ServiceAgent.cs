using SmartService.Domain.ValueObjects;
using SmartService.Domain.Exceptions;

namespace SmartService.Domain.Entities;

/// <summary>
/// Represents a service agent (technician, consultant, or professional)
/// who is eligible to execute service requests.
/// 
/// A ServiceAgent is linked to a User account and owns a set of
/// AgentCapabilities that define what types of services and
/// complexity levels they can handle.
/// 
/// ServiceAgent does NOT:
/// - Evaluate service complexity
/// - Decide assignment by itself
/// 
/// All assignments are coordinated by system or staff roles.
/// </summary>
public class ServiceAgent
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string FullName { get; private set; }
    public bool IsActive { get; private set; }
    public Money Balance { get; private set; } = Money.Create(0, "VND");

    private readonly List<AgentCapability> _capabilities = new();
    public IReadOnlyCollection<AgentCapability> Capabilities => _capabilities;

    private ServiceAgent() { }

    private ServiceAgent(Guid id, string fullName, Guid? userId = null)
    {
        Id = id;
        FullName = fullName;
        IsActive = true;
        UserId = userId;
    }

    public static ServiceAgent Create(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ServiceAgentException.AgentNameRequiredException();

        return new ServiceAgent(Guid.NewGuid(), fullName);
    }

    public static ServiceAgent CreateForUser(string fullName, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ServiceAgentException.AgentNameRequiredException();

        return new ServiceAgent(Guid.NewGuid(), fullName, userId);
    }

    public void LinkToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (UserId.HasValue && UserId.Value != userId)
            throw new InvalidOperationException("This service agent is already linked to another user.");

        UserId ??= userId;
    }

    public void AddCapability(AgentCapability capability)
    {
        capability.AssignToAgent(Id);
        _capabilities.Add(capability);
    }

    public void ReplaceCapabilities(IEnumerable<AgentCapability> capabilities)
    {
        _capabilities.Clear();
        foreach (var capability in capabilities)
        {
            AddCapability(capability);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void AddBalance(Money amount)
    {
        Balance += amount;
    }
}
