using MediatR;

namespace SmartService.Application.Features.Services.Commands.UpdateServiceDefinition;

/// <summary>
/// Command to update an existing service definition.
/// </summary>
public record UpdateServiceDefinitionCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,
    int EstimatedDuration,
    bool IsActive) : IRequest;
