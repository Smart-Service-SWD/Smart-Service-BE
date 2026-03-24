using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.PriceAdjustments.Commands.CreatePriceAdjustmentRequest;

public class CreatePriceAdjustmentRequestHandler : IRequestHandler<CreatePriceAdjustmentRequestCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreatePriceAdjustmentRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreatePriceAdjustmentRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        if (serviceRequest.Status != ServiceStatus.InProgress)
            throw new InvalidOperationException("Chỉ có thể yêu cầu điều chỉnh giá khi đơn hàng đang thực hiện.");

        var adjustmentRequest = new PriceAdjustmentRequest(
            request.ServiceRequestId,
            serviceRequest.EstimatedCost ?? Money.Create(0),
            request.NewPrice,
            request.Reason,
            request.CreatedBy);

        _context.PriceAdjustmentRequests.Add(adjustmentRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return adjustmentRequest.Id;
    }
}
