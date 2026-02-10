using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class UserQuery
{
    /// <summary>
    /// Lấy danh sách tất cả người dùng trong hệ thống (khách hàng, nhân viên, nhà cung cấp, admin).
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getUsers")]
    [GraphQLDescription("Lấy danh sách tất cả người dùng trong hệ thống. Yêu cầu quyền: Staff hoặc Admin.")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<User>> GetUsers(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Users
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một người dùng theo ID (email, tên, số điện thoại, vai trò).
    /// Yêu cầu: Đã đăng nhập (authenticated).
    /// </summary>
    [GraphQLName("getUserById")]
    [GraphQLDescription("Lấy thông tin chi tiết của một người dùng theo ID. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<User?> GetUserById(
        [GraphQLDescription("ID của người dùng cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách người dùng theo vai trò (Customer, Staff, Agent, Admin).
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getUsersByRole")]
    [GraphQLDescription("Lấy danh sách người dùng theo vai trò (Customer, Staff, Agent, Admin). Yêu cầu quyền: Staff hoặc Admin.")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<User>> GetUsersByRole(
        [GraphQLDescription("Vai trò cần lọc (CUSTOMER, STAFF, AGENT, ADMIN)")] UserRole role,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Users
            .AsNoTracking()
            .Where(x => x.Role == role)
            .ToListAsync();
    }
}
