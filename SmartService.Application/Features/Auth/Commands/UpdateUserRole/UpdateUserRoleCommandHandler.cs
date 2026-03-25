using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Auth.Commands.UpdateUserRole;

public sealed class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateUserRoleCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
            return false;

        var previousRole = user.Role;
        user.ChangeRole(request.Role);

        if (request.Role == UserRole.Agent)
        {
            var linkedAgent = await _context.ServiceAgents
                .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

            if (linkedAgent is null)
            {
                _context.ServiceAgents.Add(ServiceAgent.CreateForUser(user.FullName, user.Id));
            }
            else if (!linkedAgent.IsActive)
            {
                linkedAgent.Activate();
            }
        }
        else if (previousRole == UserRole.Agent)
        {
            var linkedAgents = await _context.ServiceAgents
                .Where(x => x.UserId == user.Id && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var linkedAgent in linkedAgents)
            {
                linkedAgent.Deactivate();
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

