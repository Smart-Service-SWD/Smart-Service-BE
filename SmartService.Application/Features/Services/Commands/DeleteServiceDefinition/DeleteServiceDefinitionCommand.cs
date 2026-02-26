using MediatR;

namespace SmartService.Application.Features.Services.Commands.DeleteServiceDefinition;

/// <summary>
/// Command to delete (remove) a service definition.
/// </summary>
public record DeleteServiceDefinitionCommand(Guid Id) : IRequest;
