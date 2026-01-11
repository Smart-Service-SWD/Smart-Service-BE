using HotChocolate;
using Microsoft.EntityFrameworkCore;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

public class ServiceRequestQuery
{
    public async Task<List<ServiceRequest>> GetServiceRequests(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceRequests
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
