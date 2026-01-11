namespace SmartService.Domain.Entities;

/// <summary>
/// Represents an audit log entry in the system.
/// 
/// Activity logs track important domain actions such as:
/// - Status changes
/// - Assignments
/// - Evaluations
///
/// This entity is used for auditing, monitoring,
/// and compliance purposes.
/// </summary>
public class ActivityLog
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public string Action { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ActivityLog() { }

    private ActivityLog(Guid serviceRequestId, string action)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        Action = action;
        CreatedAt = DateTime.UtcNow;
    }

    public static ActivityLog Create(Guid serviceRequestId, string action)
    {
        return new ActivityLog(serviceRequestId, action);
    }
}
