using MediatR;

namespace SmartService.Application.Features.Services.Commands.CreateServiceDefinition;

/// <summary>
/// Command to create a new service definition within a category.
/// </summary>
public record CreateServiceDefinitionCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal BasePrice,
    int EstimatedDuration) : IRequest<Guid>;
