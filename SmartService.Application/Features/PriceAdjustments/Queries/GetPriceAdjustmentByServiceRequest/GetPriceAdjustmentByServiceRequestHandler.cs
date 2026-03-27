using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;

namespace SmartService.Application.Features.PriceAdjustments.Queries.GetPriceAdjustmentByServiceRequest;

public class GetPriceAdjustmentByServiceRequestHandler : IRequestHandler<GetPriceAdjustmentByServiceRequestQuery, PriceAdjustmentDto?>
{
    private readonly IAppDbContext _context;

    public GetPriceAdjustmentByServiceRequestHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PriceAdjustmentDto?> Handle(GetPriceAdjustmentByServiceRequestQuery request, CancellationToken cancellationToken)
    {
        var adjustment = await _context.PriceAdjustmentRequests
            .Where(p => p.ServiceRequestId == request.ServiceRequestId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (adjustment == null) return null;

        return new PriceAdjustmentDto
        {
            Id = adjustment.Id,
            ServiceRequestId = adjustment.ServiceRequestId,
                OldPriceAmount = adjustment.OldPrice.Amount,
                OldPriceCurrency = adjustment.OldPrice.Currency,
                NewPriceAmount = adjustment.NewPrice.Amount,
                NewPriceCurrency = adjustment.NewPrice.Currency,
                Reason = adjustment.Reason,
                EvidenceImageUrl = adjustment.EvidenceImageUrl,
            Status = adjustment.Status,
            CreatedAt = adjustment.CreatedAt,
            CreatedBy = adjustment.CreatedBy
        };
    }
}
