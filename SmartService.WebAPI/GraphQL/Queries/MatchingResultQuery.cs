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
public class MatchingResultQuery
{
    /// <summary>
    /// Lấy danh sách tất cả kết quả khớp dịch vụ trong hệ thống (kết quả so khớp giữa yêu cầu dịch vụ và nhà cung cấp).
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getMatchingResults")]
    [GraphQLDescription(
        "Lấy danh sách tất cả kết quả khớp dịch vụ trong hệ thống (kết quả so khớp giữa yêu cầu dịch vụ và nhà cung cấp).\n" +
        "Yêu cầu quyền: Staff hoặc Admin.\n" +
        "Tags: Admin Dashboard, AI Matching, Staff Workflow")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<MatchingResult>> GetMatchingResults(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một kết quả khớp dịch vụ theo ID (điểm khớp, nhà cung cấp được khuyến nghị).
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getMatchingResultById")]
    [GraphQLDescription(
        "Lấy thông tin chi tiết của một kết quả khớp dịch vụ theo ID (điểm khớp, nhà cung cấp được khuyến nghị).\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: AI Matching, Detail View")]
    [Authorize]
    public async Task<MatchingResult?> GetMatchingResultById(
        [GraphQLDescription("ID của kết quả khớp dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Lấy danh sách kết quả khớp dịch vụ của một yêu cầu dịch vụ cụ thể, sắp xếp theo điểm khớp từ cao xuống thấp.
    /// Yêu cầu: Đã đăng nhập.
    /// </summary>
    [GraphQLName("getMatchingResultsByServiceRequestId")]
    [GraphQLDescription(
        "Lấy danh sách kết quả khớp dịch vụ của một yêu cầu dịch vụ cụ thể, sắp xếp theo điểm khớp từ cao xuống thấp.\n" +
        "Yêu cầu quyền: Đã đăng nhập.\n" +
        "Tags: AI Matching, Service Request Detail")]
    [Authorize]
    public async Task<List<MatchingResult>> GetMatchingResultsByServiceRequestId(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy danh sách kết quả khớp")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.MatchingScore)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách các nhà cung cấp dịch vụ được khuyến nghị cho một yêu cầu dịch vụ cụ thể, sắp xếp theo điểm khớp từ cao xuống thấp.
    /// Yêu cầu quyền: Staff hoặc Admin.
    /// </summary>
    [GraphQLName("getRecommendedMatches")]
    [GraphQLDescription(
        "Lấy danh sách các nhà cung cấp dịch vụ được khuyến nghị cho một yêu cầu dịch vụ cụ thể, sắp xếp theo điểm khớp từ cao xuống thấp.\n" +
        "Yêu cầu quyền: Staff hoặc Admin.\n" +
        "Tags: AI Matching, Staff Workflow, Recommendation")]
    [Authorize(Roles = new[] { UserRoleConstants.Staff, UserRoleConstants.Admin })]
    public async Task<List<MatchingResult>> GetRecommendedMatches(
        [GraphQLDescription("ID của yêu cầu dịch vụ cần lấy danh sách nhà cung cấp được khuyến nghị")] Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.MatchingResults
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId && x.IsRecommended)
            .OrderByDescending(x => x.MatchingScore)
            .ToListAsync();
    }
}
