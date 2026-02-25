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
public class ServiceAttachmentQuery
{
    /// <summary>
    /// Lấy danh sách tất cả các tệp đính kèm dịch vụ trong hệ thống.
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getServiceAttachments")]
    [GraphQLDescription(
        "Lấy danh sách tất cả các tệp đính kèm dịch vụ trong hệ thống.\n" +
        "Yêu cầu quyền: Staff hoặc Admin.\n" +
        "Tags: Admin Dashboard, File Management")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<ServiceAttachment>> GetServiceAttachments(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAttachments
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một tệp đính kèm dịch vụ theo ID (tên tệp, đường dẫn, loại tệp).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getServiceAttachmentById")]
    [GraphQLDescription(
        "Lấy thông tin chi tiết của một tệp đính kèm dịch vụ theo ID (tên tệp, đường dẫn, loại tệp).\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: File Management, Detail View")]
    [Authorize]
    public async Task<ServiceAttachment?> GetServiceAttachmentById(
        [GraphQLDescription("ID của tệp đính kèm dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách các tệp đính kèm của một yêu cầu dịch vụ cụ thể, sắp xếp theo thời gian tải lên mới nhất.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getAttachmentsByServiceRequestId")]
    [GraphQLDescription(
        "Lấy danh sách các tệp đính kèm của một yêu cầu dịch vụ cụ thể, sắp xếp theo thời gian tải lên mới nhất.\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: Service Request Detail, File Management")]
    [Authorize]
    public async Task<List<ServiceAttachment>> GetAttachmentsByServiceRequestId(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy danh sách tệp đính kèm")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceAttachments
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }
}
