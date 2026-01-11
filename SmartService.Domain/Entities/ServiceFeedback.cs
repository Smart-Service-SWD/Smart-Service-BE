namespace SmartService.Domain.Entities;

/// <summary>
/// Represents feedback provided after a service request is completed.
/// 
/// Feedback includes rating and optional comments
/// and is used to evaluate service quality,
/// agent performance, and system reliability.
/// </summary>
public class ServiceFeedback
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public int Rating { get; private set; } // 1–5
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ServiceFeedback() { }

    private ServiceFeedback(
        Guid serviceRequestId,
        Guid createdByUserId,
        int rating,
        string? comment)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        CreatedByUserId = createdByUserId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public static ServiceFeedback Create(
        Guid serviceRequestId,
        Guid createdByUserId,
        int rating,
        string? comment)
        => new(serviceRequestId, createdByUserId, rating, comment);
}
