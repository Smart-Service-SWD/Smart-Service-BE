using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.API.GraphQL.Types;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

/// <summary>
/// GraphQL queries for service definitions.
/// Provides service listing with category name and booking count.
/// </summary>
[ExtendObjectType(typeof(Query))]
public class ServiceDefinitionQuery
{
    /// <summary>
    /// Lấy danh sách tất cả dịch vụ, bao gồm tên danh mục và số lượng booking.
    /// Public query - không cần đăng nhập.
    /// </summary>
    [GraphQLName("getServiceDefinitions")]
    [GraphQLDescription(
        "Lấy danh sách tất cả dịch vụ trong hệ thống, bao gồm tên danh mục, giá, thời gian ước tính và số lượng booking.\n" +
        "Yêu cầu quyền: Không yêu cầu đăng nhập (Public).\n" +
        "Tags: Admin Dashboard, Service Management, Home Screen")]
    public async Task<List<ServiceListItem>> GetServiceDefinitions(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var result = await (
            from sd in db.ServiceDefinitions.AsNoTracking()
            join sc in db.ServiceCategories.AsNoTracking()
                on sd.CategoryId equals sc.Id
            select new ServiceListItem
            {
                Id = sd.Id,
                Name = sd.Name,
                Description = sd.Description,
                CategoryName = sc.Name,
                BasePrice = sd.BasePrice,
                EstimatedDuration = sd.EstimatedDuration,
                IsActive = sd.IsActive,
                BookingCount = db.ServiceRequests.Count(r => r.CategoryId == sd.CategoryId),
                CreatedAt = sd.CreatedAt,
                UpdatedAt = sd.UpdatedAt
            }).ToListAsync();

        return result;
    }

    /// <summary>
    /// Lấy chi tiết một dịch vụ theo ID.
    /// Public query - không cần đăng nhập.
    /// </summary>
    [GraphQLName("getServiceDefinitionById")]
    [GraphQLDescription(
        "Lấy chi tiết một dịch vụ theo ID, bao gồm tên danh mục, giá, thời gian ước tính và số lượng booking.\n" +
        "Yêu cầu quyền: Không yêu cầu đăng nhập (Public).\n" +
        "Tags: Admin Dashboard, Service Detail, Home Screen")]
    public async Task<ServiceListItem?> GetServiceDefinitionById(
        [GraphQLDescription("ID của dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var result = await (
            from sd in db.ServiceDefinitions.AsNoTracking()
            join sc in db.ServiceCategories.AsNoTracking()
                on sd.CategoryId equals sc.Id
            where sd.Id == id
            select new ServiceListItem
            {
                Id = sd.Id,
                Name = sd.Name,
                Description = sd.Description,
                CategoryName = sc.Name,
                BasePrice = sd.BasePrice,
                EstimatedDuration = sd.EstimatedDuration,
                IsActive = sd.IsActive,
                BookingCount = db.ServiceRequests.Count(r => r.CategoryId == sd.CategoryId),
                CreatedAt = sd.CreatedAt,
                UpdatedAt = sd.UpdatedAt
            }).FirstOrDefaultAsync();

        return result;
    }

    /// <summary>
    /// Lấy danh sách dịch vụ theo danh mục.
    /// Public query - không cần đăng nhập.
    /// </summary>
    [GraphQLName("getServiceDefinitionsByCategory")]
    [GraphQLDescription(
        "Lấy danh sách dịch vụ theo danh mục, bao gồm tên danh mục, giá, thời gian ước tính và số lượng booking.\n" +
        "Yêu cầu quyền: Không yêu cầu đăng nhập (Public).\n" +
        "Tags: Admin Dashboard, Service Management, Category Filter")]
    public async Task<List<ServiceListItem>> GetServiceDefinitionsByCategory(
        [GraphQLDescription("ID của danh mục dịch vụ")] Guid categoryId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var result = await (
            from sd in db.ServiceDefinitions.AsNoTracking()
            join sc in db.ServiceCategories.AsNoTracking()
                on sd.CategoryId equals sc.Id
            where sd.CategoryId == categoryId
            select new ServiceListItem
            {
                Id = sd.Id,
                Name = sd.Name,
                Description = sd.Description,
                CategoryName = sc.Name,
                BasePrice = sd.BasePrice,
                EstimatedDuration = sd.EstimatedDuration,
                IsActive = sd.IsActive,
                BookingCount = db.ServiceRequests.Count(r => r.CategoryId == sd.CategoryId),
                CreatedAt = sd.CreatedAt,
                UpdatedAt = sd.UpdatedAt
            }).ToListAsync();

        return result;
    }
}
