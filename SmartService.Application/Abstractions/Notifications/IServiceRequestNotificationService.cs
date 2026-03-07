namespace SmartService.Application.Abstractions.Notifications;

/// <summary>
/// Service for sending real-time notifications about service requests.
/// </summary>
public interface IServiceRequestNotificationService
{
    Task SendSafetyAdviceAsync(
        Guid serviceRequestId,
        string safetyAdvice,
        int urgencyLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a danger alert to supervisors when a service request is flagged as dangerous.
    /// Called after SaveChanges so the request is already persisted.
    /// </summary>
    Task NotifyDangerFlaggedAsync(
        Guid serviceRequestId,
        string? riskExplanation,
        CancellationToken cancellationToken = default);
}

