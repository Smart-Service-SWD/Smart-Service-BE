using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class UserQuery
{
    public async Task<List<User>> GetUsers(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetUserById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<User>> GetUsersByRole(
        UserRole role,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Users
            .AsNoTracking()
            .Where(x => x.Role == role)
            .ToListAsync();
    }
}
