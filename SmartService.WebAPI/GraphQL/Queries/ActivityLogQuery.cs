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
public class ActivityLogQuery
{
    /// <summary>
    /// Lấy danh sách tất cả nhật ký hoạt động trong hệ thống, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getActivityLogs")]
    [GraphQLDescription("Lấy danh sách tất cả nhật ký hoạt động trong hệ thống, sắp xếp theo thời gian tạo mới nhất. Yêu cầu quyền: Staff hoặc Admin.")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<ActivityLog>> GetActivityLogs(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ActivityLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một nhật ký hoạt động theo ID (hành động, thời gian, người thực hiện).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getActivityLogById")]
    [GraphQLDescription("Lấy thông tin chi tiết của một nhật ký hoạt động theo ID. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<ActivityLog?> GetActivityLogById(
        [GraphQLDescription("ID của nhật ký hoạt động cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ActivityLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách nhật ký hoạt động của một yêu cầu dịch vụ cụ thể, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getActivityLogsByServiceRequestId")]
    [GraphQLDescription("Lấy danh sách nhật ký hoạt động của một yêu cầu dịch vụ cụ thể, sắp xếp theo thời gian tạo mới nhất. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<List<ActivityLog>> GetActivityLogsByServiceRequestId(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy danh sách nhật ký hoạt động")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ActivityLogs
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
