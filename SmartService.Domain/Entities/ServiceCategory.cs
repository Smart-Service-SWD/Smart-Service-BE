using SmartService.Domain.Exceptions;

namespace SmartService.Domain.Entities;

/// <summary>
/// Represents a service category that defines the type of service
/// offered within the platform.
/// 
/// Service categories are used to:
/// - Classify service requests
/// - Group agent capabilities
/// - Support searching, filtering, and routing logic
/// 
/// Categories can be hierarchical, allowing parent-child relationships
/// to support complex domain structures across multiple industries.
/// </summary>
public class ServiceCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private ServiceCategory() { }

    private ServiceCategory(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public static ServiceCategory Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        return new ServiceCategory(Guid.NewGuid(), name, description);
    }
}