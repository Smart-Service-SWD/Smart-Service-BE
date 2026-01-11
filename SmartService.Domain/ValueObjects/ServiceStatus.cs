namespace SmartService.Domain.ValueObjects;

/// <summary>
/// Represents the lifecycle status of a service request.
/// </summary>
public enum ServiceStatus
{
    Created = 1,
    PendingReview = 2,
    Approved = 3,
    Assigned = 4,
    InProgress = 5,
    Completed = 6,
    Cancelled = 7
}
