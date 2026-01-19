using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceCategories.Commands.Create;

/// <summary>
/// Handler for CreateServiceCategoryCommand.
/// </summary>
public class CreateServiceCategoryHandler : IRequestHandler<CreateServiceCategoryCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateServiceCategoryHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = ServiceCategory.Create(request.Name, request.Description);

        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

