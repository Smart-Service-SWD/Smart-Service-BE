using MediatR;
using SmartService.Application.Abstractions.Persistence;

namespace SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

/// <summary>
/// Handler for StartServiceRequestCommand.
/// Moves an assigned request into the InProgress state.
/// </summary>
public class StartServiceRequestHandler
    : IRequestHandler<StartServiceRequestCommand, ServiceRequestStatusResult>
{
    private readonly IAppDbContext _context;

    public StartServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<ServiceRequestStatusResult> Handle(
        StartServiceRequestCommand request,
        CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException(
                $"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        serviceRequest.Start();

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceRequestStatusResult(serviceRequest.Id, serviceRequest.Status.ToString());
    }
}
