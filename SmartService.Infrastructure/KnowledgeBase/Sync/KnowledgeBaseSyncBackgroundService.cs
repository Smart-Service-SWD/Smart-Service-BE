using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.KnowledgeBase;

namespace SmartService.Infrastructure.KnowledgeBase.Sync;

/// <summary>
/// Background service that syncs KnowledgeBase JSON files from DB on application startup.
/// Ensures AI always has fresh data when the app starts.
/// </summary>
public class KnowledgeBaseSyncBackgroundService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KnowledgeBaseSyncBackgroundService> _logger;

    public KnowledgeBaseSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<KnowledgeBaseSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("KnowledgeBase sync starting on application startup...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IKnowledgeBaseSyncService>();
            await syncService.SyncAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KnowledgeBase sync failed on startup. Application will continue without sync.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
