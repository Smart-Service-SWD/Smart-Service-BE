using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateProfileCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            throw new AuthException.UserNotFoundException();
        }

        // Update domain entity
        user.UpdateProfile(request.FullName, request.PhoneNumber);

        // Save
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
