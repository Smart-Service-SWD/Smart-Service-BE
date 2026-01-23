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
public class AssignmentQuery
{
    public async Task<List<Assignment>> GetAssignments(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<Assignment?> GetAssignmentById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Assignment>> GetAssignmentsByServiceRequestId(
        Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetAssignmentsByAgentId(
        Guid agentId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .Where(x => x.AgentId == agentId)
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync();
    }
}
