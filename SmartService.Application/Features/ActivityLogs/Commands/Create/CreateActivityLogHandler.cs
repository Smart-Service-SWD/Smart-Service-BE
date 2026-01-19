using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ActivityLogs.Commands.Create;

/// <summary>
/// Handler for CreateActivityLogCommand.
/// Creates an audit log entry for domain actions.
/// </summary>
public class CreateActivityLogHandler : IRequestHandler<CreateActivityLogCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateActivityLogHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateActivityLogCommand request, CancellationToken cancellationToken)
    {
        var log = ActivityLog.Create(request.ServiceRequestId, request.Action);

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return log.Id;
    }
}

