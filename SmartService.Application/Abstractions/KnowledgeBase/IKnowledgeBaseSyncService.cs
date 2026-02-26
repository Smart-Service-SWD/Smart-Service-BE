namespace SmartService.Application.Abstractions.KnowledgeBase;

/// <summary>
/// Interface for synchronizing service data from DB to KnowledgeBase JSON files.
/// Used by BackgroundService (startup) and MediatR notification handler (on change).
/// </summary>
public interface IKnowledgeBaseSyncService
{
    /// <summary>
    /// Loads all ServiceDefinitions and ServiceCategories from DB,
    /// then writes the data to KnowledgeBase JSON files for AI consumption.
    /// </summary>
    Task SyncAllAsync(CancellationToken cancellationToken = default);
}
