using MediatR;
using SmartService.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.ServiceRequests.Commands.RejectCompletion;

public record RejectCompletionCommand(Guid ServiceRequestId, string? Reason) : IRequest<Unit>;

public class RejectCompletionHandler : IRequestHandler<RejectCompletionCommand, Unit>
{
    private readonly IAppDbContext _context;

    public RejectCompletionHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(RejectCompletionCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        serviceRequest.RejectCompletion();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
