using SmartService.Domain.ValueObjects;

namespace SmartService.Domain.Entities;

public class CommissionSettings
{
    public Guid Id { get; private set; }
    public Guid ServiceDefinitionId { get; private set; }
    public decimal CommissionPercent { get; private set; } // e.g. 20.0 (for 20%)
    public decimal DepositPercent { get; private set; }    // e.g. 30.0 (for 30%)
    public DateTime UpdatedAt { get; private set; }

    private CommissionSettings() { }

    public CommissionSettings(Guid serviceDefinitionId, decimal commissionPercent, decimal depositPercent)
    {
        Id = Guid.NewGuid();
        ServiceDefinitionId = serviceDefinitionId;
        CommissionPercent = commissionPercent;
        DepositPercent = depositPercent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(decimal commissionPercent, decimal depositPercent)
    {
        CommissionPercent = commissionPercent;
        DepositPercent = depositPercent;
        UpdatedAt = DateTime.UtcNow;
    }
}
