namespace SmartService.Domain.Entities;

public class CompletionEvidence
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public Guid WorkerId { get; private set; }
    public string ImageUrl { get; private set; }
    public string? Notes { get; private set; }
    public EvidenceType Type { get; private set; } // Before, After, Detail
    public DateTime CreatedAt { get; private set; }

    private CompletionEvidence() { }

    public CompletionEvidence(Guid serviceRequestId, Guid workerId, string imageUrl, EvidenceType type, string? notes = null)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        WorkerId = workerId;
        ImageUrl = imageUrl;
        Type = type;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }
}

public enum EvidenceType
{
    Before = 0,
    After = 1,
    Detail = 2
}
