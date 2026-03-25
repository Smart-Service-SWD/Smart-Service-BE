using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;

/// <summary>
/// Handler for EvaluateServiceComplexityCommand.
/// Updates the complexity level of a service request and returns a lightweight summary.
/// </summary>
public class EvaluateServiceComplexityHandler : IRequestHandler<EvaluateServiceComplexityCommand, EvaluateServiceComplexityResult>
{
    private readonly IAppDbContext _context;

    public EvaluateServiceComplexityHandler(IAppDbContext context)
        => _context = context;

    public async Task<EvaluateServiceComplexityResult> Handle(EvaluateServiceComplexityCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        // Kiểm tra giá không được thấp hơn giá niêm yết của dịch vụ
        if (request.ServiceDefinitionId.HasValue && request.EstimatedCost != null)
        {
            var definition = await _context.ServiceDefinitions.FindAsync(new object[] { request.ServiceDefinitionId.Value }, cancellationToken);
            if (definition != null && request.EstimatedCost.Amount < definition.BasePrice)
            {
                throw new ServiceRequestException.PriceTooLowException(definition.BasePrice);
            }
        }

        serviceRequest.Evaluate(request.Complexity, request.ServiceDefinitionId, request.EstimatedCost);

        await _context.SaveChangesAsync(cancellationToken);

        return new EvaluateServiceComplexityResult(
            ServiceRequestId: serviceRequest.Id,
            ComplexityLevel: serviceRequest.Complexity?.Level ?? 0,
            Status: serviceRequest.Status.ToString());
    }
}

