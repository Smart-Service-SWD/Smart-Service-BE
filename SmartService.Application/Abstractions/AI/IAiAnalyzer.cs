using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.DTOs;

namespace SmartService.Application.Abstractions.AI;

public interface IAiAnalyzer
{
    /// <summary>
    /// Legacy method – kept for backward compatibility.
    /// Use AnalyzeServiceRequestAsync for new flows.
    /// </summary>
    Task<AiAnalysisResultDto> AnalyzeAsync(
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a service request using ServiceDefinitions loaded from DB as AI context.
    /// Returns enriched result: complexity, urgency, price estimate, duration, danger flag.
    /// </summary>
    Task<ServiceRequestAnalysisResultDto> AnalyzeServiceRequestAsync(
        string description,
        Guid categoryId,
        IAppDbContext context,
        CancellationToken cancellationToken = default);
}
