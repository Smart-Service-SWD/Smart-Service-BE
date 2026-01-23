using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
[Authorize]
public class AgentCapabilityQuery
{
    public async Task<List<AgentCapability>> GetAgentCapabilities(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var agents = await db.ServiceAgents
            .AsNoTracking()
            .ToListAsync();

        return agents
            .SelectMany(x => x.Capabilities)
            .ToList();
    }

    public async Task<AgentCapability?> GetAgentCapabilityById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var agents = await db.ServiceAgents
            .AsNoTracking()
            .ToListAsync();

        return agents
            .SelectMany(x => x.Capabilities)
            .FirstOrDefault(x => x.Id == id);
    }

    public async Task<List<AgentCapability>> GetCapabilitiesByAgentId(
        Guid agentId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var agent = await db.ServiceAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == agentId);

        if (agent == null)
            return new List<AgentCapability>();

        return agent.Capabilities.ToList();
    }

    public async Task<List<AgentCapability>> GetCapabilitiesByCategoryId(
        Guid categoryId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var agents = await db.ServiceAgents
            .AsNoTracking()
            .ToListAsync();

        return agents
            .SelectMany(x => x.Capabilities)
            .Where(x => x.CategoryId == categoryId)
            .ToList();
    }
}
