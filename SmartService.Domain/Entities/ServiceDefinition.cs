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

    /// <summary>
    /// Complexity range [min, max] (1–5 scale). Used by AI as baseline.
    /// Example: [1, 3] means this service is normally low-to-medium complexity.
    /// </summary>
    public int[] ComplexityRange { get; private set; } = [1, 3];

    /// <summary>
    /// Marks whether this service type is inherently dangerous by default.
    /// When true, AI will always flag isDangerFlagged = true for requests of this type.
    /// </summary>
    public bool IsDangerous { get; private set; } = false;

    private ServiceDefinition() { }

    private ServiceDefinition(
        Guid id,
        Guid categoryId,
        string name,
        string? description,
        decimal basePrice,
        int estimatedDuration,
        int[]? complexityRange = null,
        bool isDangerous = false)
    {
        Id = id;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        BasePrice = basePrice;
        EstimatedDuration = estimatedDuration;
        ComplexityRange = complexityRange ?? [1, 3];
        IsDangerous = isDangerous;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceDefinition Create(
        Guid categoryId,
        string name,
        string? description,
        decimal basePrice,
        int estimatedDuration,
        int[]? complexityRange = null,
        bool isDangerous = false)
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
            estimatedDuration,
            complexityRange,
            isDangerous);
    }

    public void Update(
        string name,
        string? description,
        decimal basePrice,
        int estimatedDuration,
        bool isActive,
        int[]? complexityRange = null,
        bool? isDangerous = null)
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
        if (complexityRange is not null) ComplexityRange = complexityRange;
        if (isDangerous.HasValue) IsDangerous = isDangerous.Value;
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
