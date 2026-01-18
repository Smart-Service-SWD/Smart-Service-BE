using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class MatchingResultQuery
{
    public async Task<List<MatchingResult>> GetMatchingResults(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MatchingResult?> GetMatchingResultById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<MatchingResult>> GetMatchingResultsByServiceRequestId(
        Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.MatchingScore)
            .ToListAsync();
    }

    public async Task<List<MatchingResult>> GetRecommendedMatches(
        Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId && x.IsRecommended)
            .OrderByDescending(x => x.MatchingScore)
            .ToListAsync();
    }
}
