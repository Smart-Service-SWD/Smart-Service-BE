namespace SmartService.API.GraphQL.Types;

/// <summary>
/// GraphQL return type for service list items.
/// Enriches ServiceDefinition with computed fields like categoryName and bookingCount.
/// </summary>
public class ServiceListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public int EstimatedDuration { get; set; }
    public bool IsActive { get; set; }
    public int BookingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
