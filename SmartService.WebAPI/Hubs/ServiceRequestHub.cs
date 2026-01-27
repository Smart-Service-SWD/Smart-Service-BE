using Microsoft.AspNetCore.SignalR;

namespace SmartService.API.Hubs;

/// <summary>
/// SignalR hub for real-time updates on service requests.
/// Used to push SafetyAdvice to clients immediately after AI analysis.
/// </summary>
public class ServiceRequestHub : Hub
{
    public async Task JoinServiceRequestGroup(Guid serviceRequestId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ServiceRequest_{serviceRequestId}");
    }

    public async Task LeaveServiceRequestGroup(Guid serviceRequestId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ServiceRequest_{serviceRequestId}");
    }
}
