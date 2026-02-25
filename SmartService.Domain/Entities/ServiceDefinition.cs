using SmartService.Domain.Exceptions;

namespace SmartService.Domain.Entities;

/// <summary>
/// Represents a specific service offering within a service category.
/// 
/// A ServiceDefinition contains detailed information about a service
/// including pricing, estimated duration, and availability status.
/// 
/// This entity is the source of truth for service data that gets
/// synchronized to KnowledgeBase JSON files for AI consumption.
/// 
/// Examples:
/// - Category: "Hỗ trợ kỹ thuật máy tính" → Definitions: "Cài hệ điều hành", "Sửa phần cứng"
/// - Category: "Luật dân sự" → Definitions: "Tranh chấp hợp đồng", "Thừa kế"
/// </summary>
public class ServiceDefinition
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public int EstimatedDuration { get; private set; } // in minutes
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ServiceDefinition() { }

    private ServiceDefinition(
        Guid id,
        Guid categoryId,
        string name,
        string? description,
        decimal basePrice,
        int estimatedDuration)
    {
        Id = id;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        BasePrice = basePrice;
        EstimatedDuration = estimatedDuration;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceDefinition Create(
        Guid categoryId,
        string name,
        string? description,
        decimal basePrice,
        int estimatedDuration)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.", nameof(name));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        if (estimatedDuration <= 0)
            throw new ArgumentException("Estimated duration must be positive.", nameof(estimatedDuration));

        return new ServiceDefinition(
            Guid.NewGuid(),
            categoryId,
            name,
            description,
            basePrice,
            estimatedDuration);
    }

    public void Update(
        string name,
        string? description,
        decimal basePrice,
        int estimatedDuration,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.", nameof(name));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        if (estimatedDuration <= 0)
            throw new ArgumentException("Estimated duration must be positive.", nameof(estimatedDuration));

        Name = name;
        Description = description;
        BasePrice = basePrice;
        EstimatedDuration = estimatedDuration;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
