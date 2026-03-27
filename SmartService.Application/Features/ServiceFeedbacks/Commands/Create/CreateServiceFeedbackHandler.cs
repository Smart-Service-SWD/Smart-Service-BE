using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceFeedbacks.Commands.Create;

/// <summary>
/// Handler for CreateServiceFeedbackCommand.
/// Creates feedback for a completed service request.
/// </summary>
public class CreateServiceFeedbackHandler : IRequestHandler<CreateServiceFeedbackCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateServiceFeedbackHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateServiceFeedbackCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        // Rule: Only finished requests (Status >= 10: FinalPaymentPaid) can be rated.
        if ((int)serviceRequest.Status < 10)
            throw new InvalidOperationException("Chỉ có thể đánh giá các yêu cầu đã hoàn tất thanh toán.");

        var feedback = ServiceFeedback.Create(
            request.ServiceRequestId,
            request.CreatedByUserId,
            request.Rating,
            request.Comment);

        _context.ServiceFeedbacks.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}

