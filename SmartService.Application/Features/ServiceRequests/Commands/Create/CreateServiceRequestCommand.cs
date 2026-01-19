using MediatR;

namespace SmartService.Application.Features.ServiceRequests.Commands.Create;

/// <summary>
/// Command to create a new service request.
/// </summary>
public record CreateServiceRequestCommand(
    Guid CustomerId,
    Guid CategoryId,
    string Description,
    int? ComplexityLevel = null) : IRequest<Guid>;

