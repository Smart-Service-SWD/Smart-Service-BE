using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;

/// <summary>
/// Handler for AssignProviderCommand.
/// Assigns a provider to a service request and updates its status.
/// </summary>
public class AssignProviderHandler : IRequestHandler<AssignProviderCommand, Unit>
{
    private readonly IAppDbContext _context;

    public AssignProviderHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(AssignProviderCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        // Kiểm tra xem thợ có đang bận không
        var isBusy = await _context.ServiceRequests.AsNoTracking().AnyAsync(sr => 
            sr.AssignedProviderId == request.ProviderId && 
            (sr.Status == ServiceStatus.Assigned || 
             sr.Status == ServiceStatus.InProgress || 
             sr.Status == ServiceStatus.AwaitingCompletionReview), 
            cancellationToken);

        if (isBusy)
            throw new InvalidOperationException("Thợ hiện đang bận xử lý một đơn hàng khác.");

        // Kiểm tra giá không được thấp hơn giá niêm yết của dịch vụ (nếu đã chọn dịch vụ)
        if (serviceRequest.ServiceDefinitionId.HasValue)
        {
            var definition = await _context.ServiceDefinitions.FindAsync(new object[] { serviceRequest.ServiceDefinitionId.Value }, cancellationToken);
            if (definition != null && request.EstimatedCost.Amount < definition.BasePrice)
            {
                throw new ServiceRequestException.PriceTooLowException(definition.BasePrice);
            }
        }

        serviceRequest.AssignProvider(request.ProviderId, request.EstimatedCost);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

