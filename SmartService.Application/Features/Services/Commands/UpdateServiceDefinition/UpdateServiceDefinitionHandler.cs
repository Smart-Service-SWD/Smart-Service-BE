using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Application.Features.Services.Events;

namespace SmartService.Application.Features.Services.Commands.UpdateServiceDefinition;

/// <summary>
/// Handler for UpdateServiceDefinitionCommand.
/// Updates an existing ServiceDefinition and publishes a change notification for JSON sync.
/// </summary>
public class UpdateServiceDefinitionHandler : IRequestHandler<UpdateServiceDefinitionCommand>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public UpdateServiceDefinitionHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task Handle(UpdateServiceDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _context.ServiceDefinitions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"ServiceDefinition with ID '{request.Id}' not found.");

        definition.Update(
            request.Name,
            request.Description,
            request.BasePrice,
            request.EstimatedDuration,
            request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new ServiceDefinitionChangedNotification(), cancellationToken);
    }
}
