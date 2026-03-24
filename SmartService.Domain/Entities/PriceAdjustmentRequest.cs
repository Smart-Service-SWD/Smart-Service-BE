using SmartService.Domain.ValueObjects;

namespace SmartService.Domain.Entities;

public enum PriceAdjustmentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class PriceAdjustmentRequest
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public Money OldPrice { get; private set; }
    public Money NewPrice { get; private set; }
    public string Reason { get; private set; }
    public PriceAdjustmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public Guid? ProcessedBy { get; private set; }

    private PriceAdjustmentRequest() { }

    public PriceAdjustmentRequest(
        Guid serviceRequestId,
        Money oldPrice,
        Money newPrice,
        string reason,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Reason = reason;
        Status = PriceAdjustmentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void Approve(Guid processedBy)
    {
        if (Status != PriceAdjustmentStatus.Pending) return;
        Status = PriceAdjustmentStatus.Approved;
        ProcessedBy = processedBy;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject(Guid processedBy)
    {
        if (Status != PriceAdjustmentStatus.Pending) return;
        Status = PriceAdjustmentStatus.Rejected;
        ProcessedBy = processedBy;
        ProcessedAt = DateTime.UtcNow;
    }
}
