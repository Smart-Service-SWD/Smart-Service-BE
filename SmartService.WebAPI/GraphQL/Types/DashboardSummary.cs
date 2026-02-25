namespace SmartService.API.GraphQL.Types;

/// <summary>
/// GraphQL return type for dashboard summary data.
/// Not a domain entity — used only for the getDashboardSummary query response.
/// </summary>
public class DashboardSummary
{
    public int TotalUsers { get; set; }
    public int TotalStaff { get; set; }
    public int TotalAgents { get; set; }
    public int TotalServices { get; set; }
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedRequests { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
}
