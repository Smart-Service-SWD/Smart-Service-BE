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
}
