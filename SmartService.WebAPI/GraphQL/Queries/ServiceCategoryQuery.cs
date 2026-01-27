using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ServiceCategoryQuery
{
    /// <summary>
    /// Lấy danh sách tất cả các danh mục dịch vụ trong hệ thống (ví dụ: Pháp lý, Bất động sản, Kỹ thuật).
    /// Public query - không cần đăng nhập.
    /// </summary>
    [GraphQLName("getServiceCategories")]
    [GraphQLDescription("Lấy danh sách tất cả các danh mục dịch vụ trong hệ thống. Public query - không cần đăng nhập.")]
    public async Task<List<ServiceCategory>> GetServiceCategories(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceCategories
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một danh mục dịch vụ theo ID (tên, mô tả).
    /// Public query - không cần đăng nhập.
    /// </summary>
    [GraphQLName("getServiceCategoryById")]
    [GraphQLDescription("Lấy thông tin chi tiết của một danh mục dịch vụ theo ID. Public query - không cần đăng nhập.")]
    public async Task<ServiceCategory?> GetServiceCategoryById(
        [GraphQLDescription("ID của danh mục dịch vụ cần lấy thông tin")] Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
