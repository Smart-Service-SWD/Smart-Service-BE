using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.PriceAdjustments.Commands.ApprovePriceAdjustment;

public class ApprovePriceAdjustmentHandler : IRequestHandler<ApprovePriceAdjustmentCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ApprovePriceAdjustmentHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(ApprovePriceAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustmentRequest = await _context.PriceAdjustmentRequests
            .FirstOrDefaultAsync(ar => ar.Id == request.AdjustmentRequestId, cancellationToken);

        if (adjustmentRequest == null)
            throw new KeyNotFoundException($"PriceAdjustmentRequest with ID '{request.AdjustmentRequestId}' not found.");

        if (adjustmentRequest.Status != PriceAdjustmentStatus.Pending)
            throw new InvalidOperationException("Yêu cầu điều chỉnh giá này đã được xử lý.");

        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == adjustmentRequest.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{adjustmentRequest.ServiceRequestId}' not found.");

        // Duyệt yêu cầu
        adjustmentRequest.Approve(request.ProcessedBy);
        
        // Cập nhật giá cuối cùng cho đơn hàng
        serviceRequest.ApplyPriceAdjustment(adjustmentRequest.NewPrice);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
