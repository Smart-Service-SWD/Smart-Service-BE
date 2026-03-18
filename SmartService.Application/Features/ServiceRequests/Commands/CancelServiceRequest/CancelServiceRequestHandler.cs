using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;

namespace SmartService.Application.Features.ServiceRequests.Commands.CancelServiceRequest;

/// <summary>
/// Handler for CancelServiceRequestCommand.
/// Allows a customer to cancel their own request before staff confirms complexity.
/// </summary>
public class CancelServiceRequestHandler
    : IRequestHandler<CancelServiceRequestCommand, ServiceRequestStatusResult>
{
    private readonly IAppDbContext _context;

    public CancelServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<ServiceRequestStatusResult> Handle(
        CancelServiceRequestCommand request,
        CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException(
                $"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        if (serviceRequest.CustomerId != request.CustomerId)
            throw new UnauthorizedAccessException();

        serviceRequest.CancelByCustomer();

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceRequestStatusResult(serviceRequest.Id, serviceRequest.Status.ToString());
    }
}
