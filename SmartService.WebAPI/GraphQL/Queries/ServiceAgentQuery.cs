using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ServiceAgentQuery
{
    public async Task<List<ServiceAgent>> GetServiceAgents(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAgents
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceAgent?> GetServiceAgentById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ServiceAgent>> GetActiveServiceAgents(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAgents
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync();
    }
}
