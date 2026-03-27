using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.PriceAdjustments.Commands.RejectPriceAdjustment;

public class RejectPriceAdjustmentHandler : IRequestHandler<RejectPriceAdjustmentCommand>
{
    private readonly IAppDbContext _context;

    public RejectPriceAdjustmentHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectPriceAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustmentRequest = await _context.PriceAdjustmentRequests
            .FirstOrDefaultAsync(ar => ar.Id == request.AdjustmentRequestId, cancellationToken);

        if (adjustmentRequest == null)
            throw new Exception("Price Adjustment Request not found");

        if (adjustmentRequest.Status != PriceAdjustmentStatus.Pending)
            throw new Exception("Price Adjustment Request is not pending");

        adjustmentRequest.Reject(request.ProcessedBy);
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
