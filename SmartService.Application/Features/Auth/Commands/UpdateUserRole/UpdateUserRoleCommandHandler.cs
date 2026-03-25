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
                var normalizedFullName = user.FullName.Trim().ToLower();
                var orphanAgents = await _context.ServiceAgents
                    .Where(x => x.UserId == null && x.FullName.ToLower() == normalizedFullName)
                    .ToListAsync(cancellationToken);

                if (orphanAgents.Count == 1)
                {
                    linkedAgent = orphanAgents[0];
                    linkedAgent.LinkToUser(user.Id);
                }
                else
                {
                    linkedAgent = ServiceAgent.CreateForUser(user.FullName, user.Id);
                    _context.ServiceAgents.Add(linkedAgent);
                }
            }

            if (!linkedAgent.IsActive)
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


