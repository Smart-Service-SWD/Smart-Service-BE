using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.Start;

/// <summary>
/// Handler for StartServiceRequestCommand.
/// Starts a service request that has been assigned to a provider.
/// </summary>
public class StartServiceRequestHandler : IRequestHandler<StartServiceRequestCommand, Unit>
{
    private readonly IAppDbContext _context;

    public StartServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(StartServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new DomainException("Service request not found.");

        serviceRequest.Start();
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
