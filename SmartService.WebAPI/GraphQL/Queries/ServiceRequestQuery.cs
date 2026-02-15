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
public class ServiceRequestQuery
{
    /// <summary>
    /// Lấy danh sách tất cả yêu cầu dịch vụ trong hệ thống, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getServiceRequests")]
    [GraphQLDescription("Lấy danh sách tất cả yêu cầu dịch vụ trong hệ thống, sắp xếp theo thời gian tạo mới nhất. Yêu cầu quyền: Staff hoặc Admin.")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin, UserRoleConstants.Agent })]
    public async Task<List<ServiceRequest>> GetServiceRequests(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceRequests
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một yêu cầu dịch vụ theo ID (mô tả, độ phức tạp, trạng thái, chi phí ước tính).
    /// Yêu cầu: Đã đăng nhập.
    /// Lưu ý: Khách hàng chỉ xem được yêu cầu của chính mình, Staff/Admin xem được tất cả.
    /// </summary>
    [GraphQLName("getServiceRequestById")]
    [GraphQLDescription("Lấy thông tin chi tiết của một yêu cầu dịch vụ theo ID. Khách hàng chỉ xem được yêu cầu của chính mình, Staff/Admin xem được tất cả. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<ServiceRequest?> GetServiceRequestById(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory,
        ClaimsPrincipal claimsPrincipal)
    {
        using var db = await factory.CreateDbContextAsync();

        var request = await db.ServiceRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null)
            return null;

        // Check if user is Staff or Admin (can view all)
        var role = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        if (role == "Staff" || role == "Admin")
            return request;

        // Customer can only view their own requests
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userId, out var userIdGuid) && request.CustomerId == userIdGuid)
            return request;

        return null; // Unauthorized
    }

    /// <summary>
    /// Lấy danh sách yêu cầu dịch vụ của một khách hàng cụ thể, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu: Đã đăng nhập.
    /// Lưu ý: Khách hàng chỉ xem được yêu cầu của chính mình, Staff/Admin xem được tất cả.
    /// </summary>
    [GraphQLName("getServiceRequestsByCustomerId")]
    [GraphQLDescription("Lấy danh sách yêu cầu dịch vụ của một khách hàng cụ thể. Khách hàng chỉ xem được yêu cầu của chính mình, Staff/Admin xem được tất cả. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<List<ServiceRequest>> GetServiceRequestsByCustomerId(
        [GraphQLDescription("ID của khách hàng cần lấy danh sách yêu cầu")] Guid customerId,
        [Service] IDbContextFactory<AppDbContext> factory,
        ClaimsPrincipal claimsPrincipal)
    {
        using var db = await factory.CreateDbContextAsync();

        var role = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

        // Customer can only view their own requests
        if (role == UserRoleConstants.Customer && Guid.TryParse(userId, out var userIdGuid) && customerId != userIdGuid)
        {
            return new List<ServiceRequest>(); // Return empty list if unauthorized
        }

        return await db.ServiceRequests
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách yêu cầu dịch vụ theo trạng thái (PENDING, PENDING_REVIEW, IN_PROGRESS, COMPLETED, CANCELLED).
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getServiceRequestsByStatus")]
    [GraphQLDescription("Lấy danh sách yêu cầu dịch vụ theo trạng thái (PENDING, PENDING_REVIEW, IN_PROGRESS, COMPLETED, CANCELLED). Yêu cầu quyền: Staff hoặc Admin.")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<ServiceRequest>> GetServiceRequestsByStatus(
        [GraphQLDescription("Trạng thái cần lọc (PENDING, PENDING_REVIEW, IN_PROGRESS, COMPLETED, CANCELLED)")] ServiceStatus status,
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
