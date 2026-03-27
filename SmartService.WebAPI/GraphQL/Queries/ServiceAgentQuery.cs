using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using SmartService.Infrastructure.Persistence;
using System.Security.Claims;

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
            .Include(x => x.Capabilities)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy hồ sơ ServiceAgent của chính tài khoản đang đăng nhập.
    /// Nếu user có role Agent nhưng chưa có hồ sơ liên kết, hệ thống sẽ tự tạo hồ sơ mới.
    /// </summary>
    [GraphQLName("getMyServiceAgent")]
    [GraphQLDescription(
        "Lấy hồ sơ ServiceAgent của chính tài khoản đang đăng nhập.\n" +
        "Nếu user có role Agent nhưng chưa có hồ sơ liên kết, hệ thống sẽ tự tạo hồ sơ mới.\n" +
        "Yêu cầu quyền: Agent.\n" +
        "Tags: Agent Dashboard, Agent Profile")]
    [Authorize(Roles = new[] { UserRoleConstants.Agent })]
    public async Task<ServiceAgent?> GetMyServiceAgent(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var userIdGuid))
        {
            return null;
        }

        using var db = await factory.CreateDbContextAsync();

        var linkedAgent = await db.ServiceAgents
            .Include(x => x.Capabilities)
            .FirstOrDefaultAsync(x => x.UserId == userIdGuid);

        if (linkedAgent is not null)
        {
            return linkedAgent;
        }

        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (user is null || user.Role != UserRole.Agent)
        {
            return null;
        }

        var createdAgent = ServiceAgent.CreateForUser(user.FullName, user.Id);
        db.ServiceAgents.Add(createdAgent);
        await db.SaveChangesAsync();

        return await db.ServiceAgents
            .AsNoTracking()
            .Include(x => x.Capabilities)
            .FirstOrDefaultAsync(x => x.Id == createdAgent.Id);
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
            .Include(x => x.Capabilities)
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
            .Include(x => x.Capabilities)
            .Where(x => x.IsActive)
            .ToListAsync();
    }
}

