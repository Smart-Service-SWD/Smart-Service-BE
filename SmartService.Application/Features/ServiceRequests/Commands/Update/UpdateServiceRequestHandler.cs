using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.Update;

/// <summary>
/// Handler for UpdateServiceRequestCommand.
/// Updates the description of a service request that hasn't been assigned yet.
/// </summary>
public class UpdateServiceRequestHandler : IRequestHandler<UpdateServiceRequestCommand, Unit>
{
    private readonly IAppDbContext _context;

    public UpdateServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(UpdateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new DomainException("Service request not found.");

        serviceRequest.Update(request.Description);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
