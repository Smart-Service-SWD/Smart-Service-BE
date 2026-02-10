using Microsoft.EntityFrameworkCore;
using SmartService.Domain.Entities;

namespace SmartService.Application.Abstractions.Persistence;

/// <summary>
/// Application abstraction for database context.
/// This interface allows Application layer to interact with persistence
/// without depending on EntityFrameworkCore implementation details.
/// 
/// All handlers should depend on this interface, not the concrete AppDbContext.
/// </summary>
public interface IAppDbContext
{
    // Entity DbSets
    DbSet<User> Users { get; }
    DbSet<ServiceAgent> ServiceAgents { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<ServiceRequest> ServiceRequests { get; }
    DbSet<ServiceAttachment> ServiceAttachments { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<MatchingResult> MatchingResults { get; }
    DbSet<ServiceFeedback> ServiceFeedbacks { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<AgentCapability> AgentCapabilities { get; }
    DbSet<ServiceAnalysis> ServiceAnalyses { get; }

    /// <summary>
    /// Saves all changes made to the database asynchronously.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
