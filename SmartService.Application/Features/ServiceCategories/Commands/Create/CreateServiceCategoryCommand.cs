using MediatR;

namespace SmartService.Application.Features.ServiceCategories.Commands.Create;

/// <summary>
/// Command to create a new service category.
/// </summary>
public record CreateServiceCategoryCommand(
    string Name,
    string Description) : IRequest<Guid>;

