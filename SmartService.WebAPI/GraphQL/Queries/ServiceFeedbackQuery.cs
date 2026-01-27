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
public class ServiceFeedbackQuery
{
    /// <summary>
    /// Lấy danh sách tất cả phản hồi/đánh giá dịch vụ trong hệ thống, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getServiceFeedbacks")]
    [GraphQLDescription("Lấy danh sách tất cả phản hồi/đánh giá dịch vụ trong hệ thống, sắp xếp theo thời gian tạo mới nhất. Yêu cầu quyền: Staff hoặc Admin.")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<ServiceFeedback>> GetServiceFeedbacks(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một phản hồi/đánh giá dịch vụ theo ID (điểm đánh giá, nhận xét).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getServiceFeedbackById")]
    [GraphQLDescription("Lấy thông tin chi tiết của một phản hồi/đánh giá dịch vụ theo ID. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<ServiceFeedback?> GetServiceFeedbackById(
        [GraphQLDescription("ID của phản hồi/đánh giá dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách phản hồi/đánh giá của một yêu cầu dịch vụ cụ thể, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getFeedbackByServiceRequestId")]
    [GraphQLDescription("Lấy danh sách phản hồi/đánh giá của một yêu cầu dịch vụ cụ thể, sắp xếp theo thời gian tạo mới nhất. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<List<ServiceFeedback>> GetFeedbackByServiceRequestId(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy danh sách phản hồi/đánh giá")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách phản hồi/đánh giá của một người dùng cụ thể, sắp xếp theo thời gian tạo mới nhất.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getFeedbackByUserId")]
    [GraphQLDescription("Lấy danh sách phản hồi/đánh giá của một người dùng cụ thể, sắp xếp theo thời gian tạo mới nhất. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<List<ServiceFeedback>> GetFeedbackByUserId(
        [GraphQLDescription("ID của người dùng cần lấy danh sách phản hồi/đánh giá")] Guid userId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Tính điểm đánh giá trung bình của một yêu cầu dịch vụ cụ thể dựa trên tất cả các phản hồi.
    /// Trả về 0 nếu không có phản hồi nào.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getAverageRatingByServiceRequestId")]
    [GraphQLDescription("Tính điểm đánh giá trung bình của một yêu cầu dịch vụ cụ thể. Trả về 0 nếu không có phản hồi nào. Yêu cầu: Đã đăng nhập.")]
    [Authorize]
    public async Task<decimal> GetAverageRatingByServiceRequestId(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần tính điểm đánh giá trung bình")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var feedbacks = await db.ServiceFeedbacks
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .ToListAsync();

        return feedbacks.Count > 0 ? (decimal)feedbacks.Average(x => x.Rating) : 0;
    }
}
