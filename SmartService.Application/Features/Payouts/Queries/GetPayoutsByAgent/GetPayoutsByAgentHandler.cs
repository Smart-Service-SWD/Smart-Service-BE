using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;

namespace SmartService.Application.Features.Payouts.Queries.GetPayoutsByAgent;

public class GetPayoutsByAgentHandler : IRequestHandler<GetPayoutsByAgentQuery, List<PayoutDto>>
{
    private readonly IAppDbContext _context;

    public GetPayoutsByAgentHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PayoutDto>> Handle(GetPayoutsByAgentQuery request, CancellationToken cancellationToken)
    {
        var payouts = await _context.Payouts
            .AsNoTracking()
            .Where(p => p.WorkerId == request.AgentId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PayoutDto(
                p.Id,
                p.ServiceRequestId,
                p.WorkerId,
                p.TotalAmount,
                p.CommissionPercent / 100m,
                p.WorkerAmount,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return payouts;
    }
}
