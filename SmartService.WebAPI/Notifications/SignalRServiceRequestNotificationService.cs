using Microsoft.AspNetCore.SignalR;
using SmartService.API.Hubs;
using SmartService.Application.Abstractions.Notifications;

namespace SmartService.API.Notifications;

public class SignalRServiceRequestNotificationService : IServiceRequestNotificationService
{
    private readonly IHubContext<ServiceRequestHub> _hubContext;

    public SignalRServiceRequestNotificationService(IHubContext<ServiceRequestHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendSafetyAdviceAsync(
        Guid serviceRequestId,
        string safetyAdvice,
        int urgencyLevel,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"ServiceRequest_{serviceRequestId}")
            .SendAsync("SafetyAdviceReceived", new
            {
                ServiceRequestId = serviceRequestId,
                SafetyAdvice = safetyAdvice,
                UrgencyLevel = urgencyLevel,
                IsCritical = urgencyLevel >= 4
            }, cancellationToken);
    }

    /// <summary>
    /// Broadcasts to the "supervisors" group so staff can act immediately.
    /// </summary>
    public async Task NotifyDangerFlaggedAsync(
        Guid serviceRequestId,
        string? riskExplanation,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group("supervisors")
            .SendAsync("DangerFlaggedAlert", new
            {
                ServiceRequestId = serviceRequestId,
                RiskExplanation  = riskExplanation,
                FlaggedAt        = DateTime.UtcNow
            }, cancellationToken);
    }
}

