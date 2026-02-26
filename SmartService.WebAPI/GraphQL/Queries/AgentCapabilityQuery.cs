using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class AgentCapabilityQuery
{
    /// <summary>
    /// Lấy danh sách tất cả khả năng/năng lực của các nhà cung cấp dịch vụ trong hệ thống.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getAgentCapabilities")]
    [GraphQLDescription(
        "Lấy danh sách tất cả khả năng/năng lực của các nhà cung cấp dịch vụ trong hệ thống.\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Agent Management, Capability Overview")]
    [Authorize]
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

    /// <summary>
    /// Lấy thông tin chi tiết của một khả năng/năng lực của nhà cung cấp dịch vụ theo ID (danh mục, độ phức tạp hỗ trợ).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getAgentCapabilityById")]
    [GraphQLDescription(
        "Lấy thông tin chi tiết của một khả năng/năng lực của nhà cung cấp dịch vụ theo ID (danh mục, độ phức tạp hỗ trợ).\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Agent Management, Capability Detail")]
    [Authorize]
    public async Task<AgentCapability?> GetAgentCapabilityById(
        [GraphQLDescription("ID của khả năng/năng lực cần lấy thông tin")] Guid id,
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

    /// <summary>
    /// Lấy danh sách tất cả khả năng/năng lực của một nhà cung cấp dịch vụ cụ thể.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getCapabilitiesByAgentId")]
    [GraphQLDescription(
        "Lấy danh sách tất cả khả năng/năng lực của một nhà cung cấp dịch vụ cụ thể.\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Agent Management, Agent Detail")]
    [Authorize]
    public async Task<List<AgentCapability>> GetCapabilitiesByAgentId(
        [GraphQLDescription("ID của nhà cung cấp dịch vụ cần lấy danh sách khả năng/năng lực")] Guid agentId,
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

    /// <summary>
    /// Lấy danh sách tất cả khả năng/năng lực của các nhà cung cấp dịch vụ trong một danh mục dịch vụ cụ thể.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getCapabilitiesByCategoryId")]
    [GraphQLDescription(
        "Lấy danh sách tất cả khả năng/năng lực của các nhà cung cấp dịch vụ trong một danh mục dịch vụ cụ thể.\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Agent Management, Category Filter")]
    [Authorize]
    public async Task<List<AgentCapability>> GetCapabilitiesByCategoryId(
        [GraphQLDescription("ID của danh mục dịch vụ cần lấy danh sách khả năng/năng lực")] Guid categoryId,
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
