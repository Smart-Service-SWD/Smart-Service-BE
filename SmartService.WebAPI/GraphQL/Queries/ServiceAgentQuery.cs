using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
[Authorize]
public class ServiceAgentQuery
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<ServiceAgent>> GetServiceAgents(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();
        return await db.ServiceAgents.AsNoTracking().ToListAsync();
    }

    [UseSingleOrDefault]
    [UseProjection]
    public async Task<ServiceAgent?> GetServiceAgentById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();
        return await db.ServiceAgents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<ServiceAgent>> GetActiveServiceAgents(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();
        return await db.ServiceAgents.AsNoTracking().Where(x => x.IsActive).ToListAsync();
    }
}
