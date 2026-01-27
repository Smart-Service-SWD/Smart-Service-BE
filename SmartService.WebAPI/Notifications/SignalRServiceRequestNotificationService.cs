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
}
