using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

[ExtendObjectType(typeof(Query))]
public class ServiceFeedbackQuery
{
    public async Task<List<ServiceFeedback>> GetServiceFeedbacks(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<ServiceFeedback?> GetServiceFeedbackById(
        Guid id,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ServiceFeedback>> GetFeedbackByServiceRequestId(
        Guid serviceRequestId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .Where(x => x.ServiceRequestId == serviceRequestId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ServiceFeedback>> GetFeedbackByUserId(
        Guid userId,
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        return await db.ServiceFeedbacks
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageRatingByServiceRequestId(
        Guid serviceRequestId,
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
