using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;

/// <summary>
/// Handler for EvaluateServiceComplexityCommand.
/// Updates the complexity level of a service request.
/// </summary>
public class EvaluateServiceComplexityHandler : IRequestHandler<EvaluateServiceComplexityCommand, Unit>
{
    private readonly IAppDbContext _context;

    public EvaluateServiceComplexityHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(EvaluateServiceComplexityCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        serviceRequest.Evaluate(request.Complexity);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

