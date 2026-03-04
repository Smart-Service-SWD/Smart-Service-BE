using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.Auth.Commands.LockUser;

public sealed class LockUserCommandHandler : IRequestHandler<LockUserCommand, bool>
{
    private readonly IAppDbContext _context;

    public LockUserCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new AuthException.UserNotFoundException();

        if (user.Role != UserRole.Staff && user.Role != UserRole.Agent)
            throw new BusinessRuleException.BusinessConstraintViolationException(
                "Role", "Only Staff or Agent accounts can be locked/unlocked.");

        if (request.IsLocked)
            user.Lock();
        else
            user.Unlock();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
