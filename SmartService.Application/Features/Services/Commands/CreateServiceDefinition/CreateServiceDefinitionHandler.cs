using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Features.Services.Events;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Services.Commands.CreateServiceDefinition;

/// <summary>
/// Handler for CreateServiceDefinitionCommand.
/// Creates a new ServiceDefinition and publishes a change notification for JSON sync.
/// </summary>
public class CreateServiceDefinitionHandler : IRequestHandler<CreateServiceDefinitionCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public CreateServiceDefinitionHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreateServiceDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = ServiceDefinition.Create(
            request.CategoryId,
            request.Name,
            request.Description,
            request.BasePrice,
            request.EstimatedDuration);

        _context.ServiceDefinitions.Add(definition);
        await _context.SaveChangesAsync(cancellationToken);

        // Trigger KnowledgeBase JSON sync
        await _mediator.Publish(new ServiceDefinitionChangedNotification(), cancellationToken);

        return definition.Id;
    }
}
