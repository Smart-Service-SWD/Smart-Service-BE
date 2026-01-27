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
    public string FullName { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<AgentCapability> _capabilities = new();
    public IReadOnlyCollection<AgentCapability> Capabilities => _capabilities;

    private ServiceAgent() { }

    private ServiceAgent(Guid id, string fullName)
    {
        Id = id;
        FullName = fullName;
        IsActive = true;
    }

    public static ServiceAgent Create(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ServiceAgentException.AgentNameRequiredException();

        return new ServiceAgent(Guid.NewGuid(), fullName);
    }

    public void AddCapability(AgentCapability capability)
    {
        _capabilities.Add(capability);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
