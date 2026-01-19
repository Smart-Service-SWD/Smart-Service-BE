using MediatR;

namespace SmartService.Application.Features.ActivityLogs.Commands.Create;

/// <summary>
/// Command to create a new activity log entry.
/// </summary>
public record CreateActivityLogCommand(
    Guid ServiceRequestId,
    string Action) : IRequest<Guid>;

