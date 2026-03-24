using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceAgents.Queries.SearchServiceAgents;

public class SearchServiceAgentsHandler : IRequestHandler<SearchServiceAgentsQuery, PagedServiceAgentsResult>
{
    private readonly IAppDbContext _context;

    public SearchServiceAgentsHandler(IAppDbContext context)
        => _context = context;

    public async Task<PagedServiceAgentsResult> Handle(SearchServiceAgentsQuery request, CancellationToken cancellationToken)
    {
        var agentsQuery = _context.ServiceAgents
            .AsNoTracking()
            .Include(x => x.Capabilities)
            .AsQueryable();

        // Lấy danh sách thợ bận
        var busyAgentIds = await _context.ServiceRequests
            .Where(sr => sr.AssignedProviderId != null && 
                        (sr.Status == ServiceStatus.Assigned || 
                         sr.Status == ServiceStatus.InProgress || 
                         sr.Status == ServiceStatus.AwaitingCompletionReview ||
                         sr.Status == ServiceStatus.CompletionApproved ||
                         sr.Status == ServiceStatus.AwaitingFinalPayment ||
                         sr.Status == ServiceStatus.FinalPaymentPaid))
            .Select(sr => sr.AssignedProviderId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allAgents = await agentsQuery.ToListAsync(cancellationToken);

        // Tính toán Score và Map sang SearchItem (logic đơn giản hóa tương tự FE)
        var items = allAgents.Select(agent => 
        {
            var isBusy = busyAgentIds.Contains(agent.Id);
            var score = 50; // Mặc định
            var busyReason = isBusy ? "Thợ đang bận xử lý đơn hàng khác" : null;
            
            // Logic cộng điểm khớp năng lực
            if (request.CategoryId.HasValue)
            {
                var capability = agent.Capabilities.FirstOrDefault(c => c.CategoryId == request.CategoryId.Value);
                if (capability != null)
                {
                    score += 20;
                    if (request.MinComplexityLevel.HasValue && capability.MaxComplexity?.Level >= request.MinComplexityLevel.Value)
                    {
                        score += 30;
                    }
                }
            }

            return new ServiceAgentSearchItem(
                agent.Id,
                agent.FullName,
                agent.IsActive,
                isBusy,
                score,
                busyReason);
        })
        .OrderByDescending(x => x.MatchingScore)
        .ThenBy(x => x.IsBusy)
        .ToList();

        var totalCount = items.Count;
        var pagedItems = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedServiceAgentsResult(
            pagedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
