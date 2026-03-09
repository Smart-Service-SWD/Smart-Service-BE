using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

namespace SmartService.Application.Features.ServiceRequests.Commands.CompleteServiceRequest;

/// <summary>
/// Handler for CompleteServiceRequestCommand.
/// Moves an in-progress request into the Completed state.
/// </summary>
public class CompleteServiceRequestHandler
    : IRequestHandler<CompleteServiceRequestCommand, ServiceRequestStatusResult>
{
    private readonly IAppDbContext _context;

    public CompleteServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<ServiceRequestStatusResult> Handle(
        CompleteServiceRequestCommand request,
        CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException(
                $"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        serviceRequest.Complete();

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceRequestStatusResult(serviceRequest.Id, serviceRequest.Status.ToString());
    }
}
