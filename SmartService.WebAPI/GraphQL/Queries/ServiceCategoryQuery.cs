using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ServiceCategoryQuery
{
    public async Task<List<ServiceCategory>> GetServiceCategories(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceCategories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceCategory?> GetServiceCategoryById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
