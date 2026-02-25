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
public class AssignmentQuery
{
    /// <summary>
    /// Lấy danh sách tất cả các phân công dịch vụ trong hệ thống, sắp xếp theo thời gian phân công mới nhất.
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getAssignments")]
    [GraphQLDescription(
        "Lấy danh sách tất cả các phân công dịch vụ trong hệ thống, sắp xếp theo thời gian phân công mới nhất.\n" +
        "Yêu cầu quyền: Staff hoặc Admin.\n" +
        "Tags: Admin Dashboard, Assignment Management, Staff Workflow")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<Assignment>> GetAssignments(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một phân công dịch vụ theo ID (yêu cầu dịch vụ, nhà cung cấp, chi phí ước tính).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getAssignmentById")]
    [GraphQLDescription(
        "Lấy thông tin chi tiết của một phân công dịch vụ theo ID (yêu cầu dịch vụ, nhà cung cấp, chi phí ước tính).\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Assignment Detail, Staff Workflow")]
    [Authorize]
    public async Task<Assignment?> GetAssignmentById(
        [GraphQLDescription("ID của phân công dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách các phân công dịch vụ của một yêu cầu dịch vụ cụ thể.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getAssignmentsByServiceRequestId")]
    [GraphQLDescription(
        "Lấy danh sách các phân công dịch vụ của một yêu cầu dịch vụ cụ thể.\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Service Request Detail, Assignment Management")]
    [Authorize]
    public async Task<List<Assignment>> GetAssignmentsByServiceRequestId(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy danh sách phân công")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.Assignments
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách các phân công dịch vụ của một nhà cung cấp dịch vụ cụ thể, sắp xếp theo thời gian phân công mới nhất.
    /// Yêu cầu quyền: Agent (chỉ xem được của mình), Staff hoặc Admin (xem được tất cả).
    /// </summary>
    [GraphQLName("getAssignmentsByAgentId")]
    [GraphQLDescription(
        "Lấy danh sách các phân công dịch vụ của một nhà cung cấp dịch vụ cụ thể, sắp xếp theo thời gian phân công mới nhất.\n" +
        "Yêu cầu quyền: Agent (chỉ xem của mình), Staff hoặc Admin (xem tất cả).\n" +
        "Tags: Agent Dashboard, Assignment Management, Agent Workflow")]
    [Authorize(Roles = new[] { UserRoleConstants.Agent, UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<Assignment>> GetAssignmentsByAgentId(
        [GraphQLDescription("ID của nhà cung cấp dịch vụ cần lấy danh sách phân công")] Guid agentId,
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
