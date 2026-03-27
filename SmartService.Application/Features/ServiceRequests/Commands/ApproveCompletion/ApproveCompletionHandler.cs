using MediatR;
using SmartService.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.ServiceRequests.Commands.ApproveCompletion;

public record ApproveCompletionCommand(Guid ServiceRequestId) : IRequest<Unit>;

public class ApproveCompletionHandler : IRequestHandler<ApproveCompletionCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ApproveCompletionHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(ApproveCompletionCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        serviceRequest.ApproveCompletion();
        
        // Auto transition to AwaitingFinalPayment as per requirements: "Chỉ sau khi staff xác nhận, hệ thống mới hiển thị QR cho customer thanh toán phần còn lại."
        serviceRequest.MarkAsAwaitingFinalPayment();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
