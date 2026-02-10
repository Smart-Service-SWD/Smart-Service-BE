using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Handler for CreateServiceRequestCommand.
/// Creates a new service request and persists it to the database.
/// </summary>
public class CreateServiceRequestHandler : IRequestHandler<CreateServiceRequestCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        // Validation is already done by ValidationBehavior pipeline
        ServiceComplexity? complexity = null;
        if (request.ComplexityLevel.HasValue)
        {
            complexity = ServiceComplexity.From(request.ComplexityLevel.Value);
        }

        var serviceRequest = ServiceRequest.Create(
            request.CustomerId,
            request.CategoryId,
            request.Description,
            request.AddressText,
            complexity);

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return serviceRequest.Id;
    }
}

