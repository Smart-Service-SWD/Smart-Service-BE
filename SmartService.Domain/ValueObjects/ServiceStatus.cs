namespace SmartService.Domain.ValueObjects;

/// <summary>
/// Represents the lifecycle status of a service request.
/// </summary>
public enum ServiceStatus
{
    AwaitingAnalysis = 0,
    Created = 1,
    PendingReview = 2,
    AwaitingDeposit = 3,
    DepositPaid = 4,
    Assigned = 5,
    InProgress = 6,
    AwaitingCompletionReview = 7,
    CompletionApproved = 8,
    AwaitingFinalPayment = 9,
    FinalPaymentPaid = 10,
    PayoutCompleted = 11,
    Cancelled = 12,
    UrgentDispatch = 13
}
