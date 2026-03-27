using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.Payouts.Commands.ProcessPayout;

public class ProcessPayoutHandler : IRequestHandler<ProcessPayoutCommand, Guid>
{
    private readonly IAppDbContext _context;

    public ProcessPayoutHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        if (serviceRequest.Status != ServiceStatus.FinalPaymentPaid)
            throw new InvalidOperationException("Chỉ có thể chia tiền sau khi khách hàng đã thanh toán hoàn tất (Trạng thái FinalPaymentPaid).");

        if (serviceRequest.AssignedProviderId == null)
            throw new InvalidOperationException("Đơn hàng chưa được gán thợ, không thể chia tiền.");

        var finalAmount = serviceRequest.FinalPrice ?? serviceRequest.EstimatedCost ?? Money.Create(0);
        
        var payout = new Payout(
            serviceRequest.Id,
            serviceRequest.AssignedProviderId.Value,
            finalAmount,
            request.CommissionPercent);

        _context.Payouts.Add(payout);
        
        // Cập nhật trạng thái đơn hàng sang PayoutCompleted (Hoàn tất toàn bộ)
        serviceRequest.MarkAsPayoutCompleted();

        await _context.SaveChangesAsync(cancellationToken);

        return payout.Id;
    }
}
