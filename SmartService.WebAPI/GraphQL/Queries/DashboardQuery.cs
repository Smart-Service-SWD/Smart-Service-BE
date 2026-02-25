using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.API.GraphQL;
using SmartService.API.GraphQL.Types;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Queries;

/// <summary>
/// GraphQL queries for admin dashboard summary.
/// Provides aggregated statistics about users, services, requests, and revenue.
/// </summary>
[ExtendObjectType(typeof(Query))]
public class DashboardQuery
{
    /// <summary>
    /// Lấy tổng quan thống kê hệ thống cho admin dashboard.
    /// Bao gồm: tổng users, staff, agents, services, requests, revenue.
    /// </summary>
    [GraphQLName("getDashboardSummary")]
    [GraphQLDescription(
        "Lấy tổng quan thống kê hệ thống: tổng users, staff, agents, services, requests, revenue.\n" +
        "Yêu cầu quyền: Staff hoặc Admin.\n" +
        "Tags: Admin Dashboard, Admin Overview")]
    [Authorize(Roles = new[] { "Staff", "Admin" })]
    public async Task<DashboardSummary> GetDashboardSummary(
        [Service] IDbContextFactory<AppDbContext> factory)
    {
        using var db = await factory.CreateDbContextAsync();

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalUsers = await db.Users.CountAsync();
        var totalStaff = await db.Users.CountAsync(u => u.Role == UserRole.Staff);
        var totalAgents = await db.ServiceAgents.CountAsync();
        var totalServices = await db.ServiceDefinitions.CountAsync(s => s.IsActive);
        var totalRequests = await db.ServiceRequests.CountAsync();
        var pendingRequests = await db.ServiceRequests.CountAsync(r => r.Status == ServiceStatus.PendingReview);
        var completedRequests = await db.ServiceRequests.CountAsync(r => r.Status == ServiceStatus.Completed);

        // Revenue from completed assignments
        var todayRevenue = await db.Assignments
            .Where(a => db.ServiceRequests.Any(r =>
                r.Id == a.ServiceRequestId &&
                r.Status == ServiceStatus.Completed &&
                r.CreatedAt >= today))
            .SumAsync(a => a.EstimatedCost.Amount);

        var monthlyRevenue = await db.Assignments
            .Where(a => db.ServiceRequests.Any(r =>
                r.Id == a.ServiceRequestId &&
                r.Status == ServiceStatus.Completed &&
                r.CreatedAt >= firstDayOfMonth))
            .SumAsync(a => a.EstimatedCost.Amount);

        return new DashboardSummary
        {
            TotalUsers = totalUsers,
            TotalStaff = totalStaff,
            TotalAgents = totalAgents,
            TotalServices = totalServices,
            TotalRequests = totalRequests,
            PendingRequests = pendingRequests,
            CompletedRequests = completedRequests,
            TodayRevenue = todayRevenue,
            MonthlyRevenue = monthlyRevenue
        };
    }
}
