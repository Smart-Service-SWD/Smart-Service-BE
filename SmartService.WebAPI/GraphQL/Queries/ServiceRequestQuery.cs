using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
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

    public async Task<ServiceRequest?> GetServiceRequestById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ServiceRequest>> GetServiceRequestsByCustomerId(
        Guid customerId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceRequests
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ServiceRequest>> GetServiceRequestsByStatus(
        ServiceStatus status,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceRequests
            .AsNoTracking()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
