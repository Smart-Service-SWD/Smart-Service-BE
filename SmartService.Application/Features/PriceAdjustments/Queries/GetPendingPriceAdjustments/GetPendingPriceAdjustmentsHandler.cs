using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;

public class GetPendingPriceAdjustmentsHandler : IRequestHandler<GetPendingPriceAdjustmentsQuery, List<PriceAdjustmentDto>>
{
    private readonly IAppDbContext _context;

    public GetPendingPriceAdjustmentsHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PriceAdjustmentDto>> Handle(GetPendingPriceAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.PriceAdjustmentRequests
            .AsNoTracking()
            .Where(x => x.Status == PriceAdjustmentStatus.Pending)
            .Select(x => new PriceAdjustmentDto
            {
                Id = x.Id,
                ServiceRequestId = x.ServiceRequestId,
                OldPriceAmount = x.OldPrice.Amount,
                OldPriceCurrency = x.OldPrice.Currency,
                NewPriceAmount = x.NewPrice.Amount,
                NewPriceCurrency = x.NewPrice.Currency,
                Reason = x.Reason,
                EvidenceImageUrl = x.EvidenceImageUrl,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
