using SmartService.Domain.ValueObjects;

namespace SmartService.Domain.Entities;

public class Payout
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public Guid WorkerId { get; private set; }
    public Money TotalAmount { get; private set; }
    public decimal CommissionPercent { get; private set; }
    public Money CommissionAmount { get; private set; }
    public Money WorkerAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? PaymentReference { get; private set; }

    private Payout() { }

    public Payout(
        Guid serviceRequestId,
        Guid workerId,
        Money totalAmount,
        decimal commissionPercent,
        string? paymentReference = null)
    {
        Id = Guid.NewGuid();
        ServiceRequestId = serviceRequestId;
        WorkerId = workerId;
        TotalAmount = totalAmount;
        CommissionPercent = commissionPercent;
        
        var commissionVal = totalAmount.Amount * (commissionPercent / 100m);
        CommissionAmount = Money.Create(commissionVal, totalAmount.Currency);
        WorkerAmount = Money.Create(totalAmount.Amount - commissionVal, totalAmount.Currency);
        
        CreatedAt = DateTime.UtcNow;
        PaymentReference = paymentReference;
    }
}
