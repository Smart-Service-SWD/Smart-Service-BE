using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Features.Services.Events;

namespace SmartService.Application.Features.Services.Commands.DeleteServiceDefinition;

/// <summary>
/// Handler for DeleteServiceDefinitionCommand.
/// Removes a ServiceDefinition and publishes a change notification for JSON sync.
/// </summary>
public class DeleteServiceDefinitionHandler : IRequestHandler<DeleteServiceDefinitionCommand>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public DeleteServiceDefinitionHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task Handle(DeleteServiceDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _context.ServiceDefinitions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"ServiceDefinition with ID '{request.Id}' not found.");

        _context.ServiceDefinitions.Remove(definition);
        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ServiceDefinitionChangedNotification(), cancellationToken);
    }
}
