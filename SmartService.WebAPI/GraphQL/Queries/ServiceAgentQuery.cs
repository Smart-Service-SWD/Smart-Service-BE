using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ServiceAgentQuery
{
    /// <summary>
    /// Lấy danh sách tất cả nhà cung cấp dịch vụ trong hệ thống (bao gồm cả active và inactive).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getServiceAgents")]
    [GraphQLDescription(
        "Lấy danh sách tất cả nhà cung cấp dịch vụ trong hệ thống (bao gồm cả active và inactive).\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Admin Dashboard, Agent Management")]
    [Authorize]
    public async Task<List<ServiceAgent>> GetServiceAgents(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAgents
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một nhà cung cấp dịch vụ theo ID (thông tin liên hệ, trạng thái, khả năng).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getServiceAgentById")]
    [GraphQLDescription(
        "Lấy thông tin chi tiết của một nhà cung cấp dịch vụ theo ID (thông tin liên hệ, trạng thái, khả năng).\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Agent Management, Agent Detail")]
    [Authorize]
    public async Task<ServiceAgent?> GetServiceAgentById(
        [GraphQLDescription("ID của nhà cung cấp dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách các nhà cung cấp dịch vụ đang hoạt động (isActive = true).
    /// Public query - không cần đăng nhập.
    /// </summary>
    [GraphQLName("getActiveServiceAgents")]
    [GraphQLDescription(
        "Lấy danh sách các nhà cung cấp dịch vụ đang hoạt động (isActive = true).\n" +
        "Yêu cầu quyền: Không yêu cầu đăng nhập (Public).\n" +
        "Tags: Home Screen, Service Booking")]
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
