using MediatR;
using SmartService.Application.Abstractions.KnowledgeBase;
using SmartService.Application.Features.Services.Events;

namespace SmartService.Application.Features.Services.Events;

/// <summary>
/// Handles ServiceDefinitionChangedNotification by triggering KnowledgeBase JSON sync.
/// Called automatically after any ServiceDefinition Create/Update/Delete operation.
/// </summary>
public class SyncKnowledgeBaseOnChangeHandler : INotificationHandler<ServiceDefinitionChangedNotification>
{
    private readonly IKnowledgeBaseSyncService _syncService;

    public SyncKnowledgeBaseOnChangeHandler(IKnowledgeBaseSyncService syncService)
    {
        _syncService = syncService;
    }

    public async Task Handle(ServiceDefinitionChangedNotification notification, CancellationToken cancellationToken)
    {
        await _syncService.SyncAllAsync(cancellationToken);
    }
}
